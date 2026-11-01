using System.Security.Cryptography;
using System.Text;
using DotNetSkeleton.Modules.AuthModule.Configuration;
using DotNetSkeleton.Modules.AuthModule.Constants;
using DotNetSkeleton.Modules.AuthModule.DTOs.Requests;
using DotNetSkeleton.Modules.AuthModule.DTOs.Responses;
using DotNetSkeleton.Modules.AuthModule.Entities;
using DotNetSkeleton.Modules.AuthModule.Enums;
using DotNetSkeleton.Modules.AuthModule.Exceptions;
using DotNetSkeleton.Modules.AuthModule.Mappings;
using DotNetSkeleton.Modules.AuthModule.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using DotNetSkeleton.Shared.Persistence;
using Mapster;
using Npgsql;

namespace DotNetSkeleton.Modules.AuthModule.Services;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<UserEntity> _passwordHasher;
    private readonly AuthTokenService _tokenService;
    private readonly IOtpSmsSender _otpSmsSender;
    private readonly JwtOptions _options;
    private readonly IWebHostEnvironment _environment;

    public AuthService(
        AppDbContext context,
        IPasswordHasher<UserEntity> passwordHasher,
        AuthTokenService tokenService,
        IOtpSmsSender otpSmsSender,
        IOptions<JwtOptions> options,
        IWebHostEnvironment environment)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _otpSmsSender = otpSmsSender;
        _options = options.Value;
        _environment = environment;
    }

    public async Task<AuthOtpChallengeResponseDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var phone = NormalizePhone(request.Phone);
        var user = await _context.Users.SingleOrDefaultAsync(x => x.Phone == phone, cancellationToken);
        if (user is { IsVerified: true })
            throw new AuthUserAlreadyRegisteredException();

        if (user is { Role: UserRole.Admin } or { IsActive: true })
            return CreateUnavailableChallenge(OtpPurpose.Registration);

        return await RegisterPendingUserAsync(request, cancellationToken);
    }

    private async Task<AuthOtpChallengeResponseDto> RegisterPendingUserAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var phone = NormalizePhone(request.Phone);
        var user = await _context.Users
            .SingleOrDefaultAsync(x => x.Phone == phone, cancellationToken);

        if (user is { IsVerified: true })
            throw new AuthUserAlreadyRegisteredException();

        if (user is { Role: UserRole.Admin } or { IsActive: true })
            return CreateUnavailableChallenge(OtpPurpose.Registration);

        var userWasCreated = false;
        if (user is null)
        {
            user = new UserEntity
            {
                Phone = phone,
                IsActive = false,
                IsVerified = false,
                Role = UserRole.User,
                PermissionId = null
            };
            // Keep a random unusable credential until the phone owner verifies the OTP.
            var pendingCredential = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
            user.Password = _passwordHasher.HashPassword(user, pendingCredential);
            _context.Users.Add(user);
            userWasCreated = true;
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            userWasCreated && IsPhoneUniqueViolation(exception))
        {
            _context.Entry(user).State = EntityState.Detached;
            var concurrentUser = await _context.Users
                .SingleOrDefaultAsync(x => x.Phone == phone, cancellationToken);
            if (concurrentUser is null)
                throw;

            if (concurrentUser.IsVerified)
                throw new AuthUserAlreadyRegisteredException();

            if (concurrentUser.Role == UserRole.Admin || concurrentUser.IsActive)
                return CreateUnavailableChallenge(OtpPurpose.Registration);

            user = concurrentUser;
        }

        return await CreateAndSendOtpAsync(user, OtpPurpose.Registration, cancellationToken);
    }

    public async Task<AuthResponseDto> VerifyRegistrationAsync(
        CompleteRegistrationRequestDto request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var otp = await _context.AuthOtps
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.PublicId == request.PublicId, cancellationToken);
        if (otp is null || otp.Purpose != OtpPurpose.Registration || otp.VerifiedAt is null ||
            otp.ExpiredAt <= now || otp.User.IsActive || otp.User.IsVerified)
            throw new AuthOtpInvalidOrExpiredException();

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var claimed = await MarkOtpConsumedAsync(otp, now, cancellationToken);
        if (!claimed)
            throw new AuthOtpInvalidOrExpiredException();

        otp.ExpiredAt = now;
        otp.User.Password = _passwordHasher.HashPassword(otp.User, request.Password);
        otp.User.IsActive = true;
        otp.User.IsVerified = true;
        otp.User.VerifiedAt = now;
        otp.User.LastActiveAt = now;
        await _context.SaveChangesAsync(cancellationToken);
        var response = await CreateSessionAsync(otp.User, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return response;
    }

    public async Task<AuthOtpChallengeResponseDto> VerifyOtpAsync(
        Guid publicId,
        VerifyOtpRequestDto request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var otp = await _context.AuthOtps
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);

        if (otp is null || otp.VerifiedAt is not null || otp.ExpiredAt <= now ||
            !IsOtpPurposeAllowedForUser(otp.User, otp.Purpose) ||
            otp.FailedAttempts >= OtpConstants.MaxFailedAttempts)
            throw new AuthOtpInvalidOrExpiredException();

        if (_passwordHasher.VerifyHashedPassword(otp.User, otp.Code, request.Code) ==
            PasswordVerificationResult.Failed)
        {
            await _context.AuthOtps
                .Where(x => x.Id == otp.Id && x.VerifiedAt == null && x.ExpiredAt > now &&
                            x.FailedAttempts < OtpConstants.MaxFailedAttempts)
                .ExecuteUpdateAsync(update => update
                    .SetProperty(x => x.FailedAttempts, x => x.FailedAttempts + 1)
                    .SetProperty(
                        x => x.ExpiredAt,
                        x => x.FailedAttempts + 1 >= OtpConstants.MaxFailedAttempts ? now : x.ExpiredAt),
                    cancellationToken);
            throw new AuthOtpInvalidOrExpiredException();
        }

        var verified = await _context.AuthOtps
            .Where(x => x.Id == otp.Id && x.VerifiedAt == null && x.ExpiredAt > now &&
                        x.FailedAttempts < OtpConstants.MaxFailedAttempts)
            .ExecuteUpdateAsync(update => update.SetProperty(x => x.VerifiedAt, now), cancellationToken);
        if (verified != 1)
            throw new AuthOtpInvalidOrExpiredException();

        otp.VerifiedAt = now;
        return ToChallengeResponse(otp);
    }

    public async Task ResendOtpAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var otp = await _context.AuthOtps
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
        if (otp is null || otp.VerifiedAt is not null || !IsOtpPurposeAllowedForUser(otp.User, otp.Purpose))
            return;

        var latestPublicId = await _context.AuthOtps
            .Where(x => x.UserId == otp.UserId && x.Purpose == otp.Purpose)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.PublicId)
            .FirstOrDefaultAsync(cancellationToken);
        if (latestPublicId != publicId)
            return;

        var now = DateTimeOffset.UtcNow;
        if (otp.CreatedAt > now.AddSeconds(-OtpConstants.ResendCooldownSeconds))
            return;

        await CreateAndSendOtpAsync(otp.User, otp.Purpose, cancellationToken);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        var user = await LoadUserForTokensAsync(NormalizePhone(request.Phone), cancellationToken);
        if (user is null || !user.IsActive || !user.IsVerified)
        {
            throw new AuthInvalidCredentialsException();
        }

        var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
        if (passwordResult == PasswordVerificationResult.Failed)
            throw new AuthInvalidCredentialsException();

        if (passwordResult == PasswordVerificationResult.SuccessRehashNeeded)
            user.Password = _passwordHasher.HashPassword(user, request.Password);

        user.LastActiveAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return await CreateSessionAsync(user, cancellationToken);
    }

    public async Task<AuthOtpChallengeResponseDto> RequestPasswordResetAsync(
        ForgotPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users.SingleOrDefaultAsync(
            x => x.Phone == NormalizePhone(request.Phone) && x.IsActive && x.IsVerified,
            cancellationToken);

        // Return the same response shape for known and unknown phone numbers.
        return user is null
            ? CreateUnavailableChallenge(OtpPurpose.PasswordReset)
            : await CreateAndSendOtpAsync(user, OtpPurpose.PasswordReset, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var otp = await _context.AuthOtps
            .Include(x => x.User)
            .SingleOrDefaultAsync(x => x.PublicId == request.PublicId, cancellationToken);
        if (otp is null || otp.Purpose != OtpPurpose.PasswordReset || otp.VerifiedAt is null ||
            otp.ExpiredAt <= now || !otp.User.IsActive || !otp.User.IsVerified)
            throw new AuthOtpInvalidOrExpiredException();

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var claimed = await MarkOtpConsumedAsync(otp, now, cancellationToken);
        if (!claimed)
            throw new AuthOtpInvalidOrExpiredException();

        otp.ExpiredAt = now;
        otp.User.Password = _passwordHasher.HashPassword(otp.User, request.NewPassword);

        var activeSessions = await _context.AuthSessions
            .Where(x => x.UserId == otp.UserId && x.RevokedAt == null && x.ExpiredAt > now)
            .ToListAsync(cancellationToken);
        foreach (var session in activeSessions)
            session.RevokedAt = now;

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var now = DateTimeOffset.UtcNow;
        var session = await _context.AuthSessions
            .Include(x => x.User)
                .ThenInclude(x => x.Permission)
                    .ThenInclude(x => x!.Details)
                        .ThenInclude(x => x.Function)
            .SingleOrDefaultAsync(x => x.RefreshToken == tokenHash, cancellationToken);

        if (session is null || session.RevokedAt is not null || session.ExpiredAt <= now ||
            !session.User.IsActive || !session.User.IsVerified)
            throw new AuthRefreshTokenInvalidException();

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        // Conditional update makes refresh-token rotation single-use under concurrent requests.
        var revoked = await _context.AuthSessions
            .Where(x => x.Id == session.Id && x.RevokedAt == null && x.ExpiredAt > now)
            .ExecuteUpdateAsync(update => update.SetProperty(x => x.RevokedAt, now), cancellationToken);
        if (revoked != 1)
            throw new AuthRefreshTokenInvalidException();

        session.User.LastActiveAt = now;
        await _context.SaveChangesAsync(cancellationToken);
        var refreshedSession = await CreateSessionAsync(session.User, cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return refreshedSession;
    }

    public async Task LogoutAsync(int userId, int sessionId, CancellationToken cancellationToken)
    {
        var session = await _context.AuthSessions.SingleOrDefaultAsync(
            x => x.Id == sessionId && x.UserId == userId,
            cancellationToken);

        if (session is not null && session.RevokedAt is null)
        {
            session.RevokedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<UserResponseDto> GetProfileAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == userId && x.IsActive && x.IsVerified, cancellationToken);

        if (user is null)
            throw new AuthAccountNotFoundException();

        return user.Adapt<UserResponseDto>(AuthMappingConfiguration.Config);
    }

    private async Task<AuthResponseDto> CreateSessionAsync(UserEntity user, CancellationToken cancellationToken)
    {
        // Permission claims must be refreshed from PostgreSQL whenever a session is created.
        user = await _context.Users
            .Include(x => x.Permission)
                .ThenInclude(x => x!.Details)
                    .ThenInclude(x => x.Function)
            .SingleAsync(x => x.Id == user.Id, cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var refreshToken = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(64));
        var session = new AuthSessionEntity
        {
            UserId = user.Id,
            RefreshToken = HashToken(refreshToken),
            CreatedAt = now,
            ExpiredAt = now.AddDays(_options.RefreshTokenDays)
        };

        _context.AuthSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        var (accessToken, accessTokenExpiresAt) = _tokenService.CreateAccessToken(user, session);
        return new AuthResponseDto(
            accessToken,
            refreshToken,
            accessTokenExpiresAt,
            user.Adapt<UserResponseDto>(AuthMappingConfiguration.Config));
    }

    private async Task<AuthOtpChallengeResponseDto> CreateAndSendOtpAsync(
        UserEntity user,
        OtpPurpose purpose,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var lockedUser = await _context.Users
            .FromSqlInterpolated($"SELECT * FROM \"user\" WHERE id = {user.Id} FOR UPDATE")
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);
        if (lockedUser is null || !IsOtpPurposeAllowedForUser(lockedUser, purpose))
        {
            await transaction.CommitAsync(cancellationToken);
            return CreateUnavailableChallenge(purpose);
        }

        var now = DateTimeOffset.UtcNow;
        var latestOtp = await _context.AuthOtps
            .Where(x => x.UserId == user.Id && x.Purpose == purpose)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var staleOtpQuery = _context.AuthOtps
            .Where(x => x.UserId == user.Id && x.Purpose == purpose && x.ExpiredAt > now);
        if (latestOtp is not null)
            staleOtpQuery = staleOtpQuery.Where(x => x.Id != latestOtp.Id);
        var staleOtps = await staleOtpQuery.ToListAsync(cancellationToken);
        foreach (var staleOtp in staleOtps)
            staleOtp.ExpiredAt = now;

        var latestOtpWasConsumed = latestOtp is { VerifiedAt: not null } && latestOtp.ExpiredAt <= now;
        if (latestOtp is not null && !latestOtpWasConsumed &&
            latestOtp.CreatedAt > now.AddSeconds(-OtpConstants.ResendCooldownSeconds))
        {
            if (staleOtps.Count > 0)
                await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ToChallengeResponse(latestOtp);
        }

        var code = _environment.IsDevelopment()
            ? OtpConstants.DevelopmentOtpCode
            : RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var canReuseLatestOtp = latestOtp is not null &&
            !(latestOtp.VerifiedAt is not null && latestOtp.ExpiredAt <= now);
        var otp = canReuseLatestOtp ? latestOtp! : new AuthOtpEntity
        {
            UserId = user.Id,
            Purpose = purpose,
        };

        otp.Code = _passwordHasher.HashPassword(lockedUser, code);
        otp.FailedAttempts = 0;
        otp.CreatedAt = now;
        otp.ExpiredAt = now.AddMinutes(_options.OtpMinutes);
        otp.VerifiedAt = null;

        if (!canReuseLatestOtp)
            _context.AuthOtps.Add(otp);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        // Development is temporarily routed to a local OTP logger; non-Development uses the configured SMS provider.
        await _otpSmsSender.SendAsync(lockedUser.Phone, code, cancellationToken);
        return ToChallengeResponse(otp);
    }

    private async Task<bool> MarkOtpConsumedAsync(
        AuthOtpEntity otp,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var consumed = await _context.AuthOtps
            .Where(x => x.Id == otp.Id && x.VerifiedAt != null && x.ExpiredAt > now)
            .ExecuteUpdateAsync(update => update.SetProperty(x => x.ExpiredAt, now), cancellationToken);
        return consumed == 1;
    }

    private AuthOtpChallengeResponseDto CreateUnavailableChallenge(OtpPurpose purpose)
    {
        var now = DateTimeOffset.UtcNow;
        return new AuthOtpChallengeResponseDto(
            Guid.NewGuid(),
            purpose,
            now,
            now.AddMinutes(_options.OtpMinutes),
            now.AddSeconds(OtpConstants.ResendCooldownSeconds),
            null);
    }

    private static AuthOtpChallengeResponseDto ToChallengeResponse(AuthOtpEntity otp) =>
        new(
            otp.PublicId,
            otp.Purpose,
            otp.CreatedAt,
            otp.ExpiredAt,
            otp.CreatedAt.AddSeconds(OtpConstants.ResendCooldownSeconds),
            otp.VerifiedAt);

    private static bool IsOtpPurposeAllowedForUser(UserEntity user, OtpPurpose purpose) =>
        purpose switch
        {
            OtpPurpose.Registration => !user.IsActive && !user.IsVerified,
            OtpPurpose.PasswordReset => user.IsActive && user.IsVerified,
            _ => false
        };

    private Task<UserEntity?> LoadUserForTokensAsync(string phone, CancellationToken cancellationToken) =>
        _context.Users
            .Include(x => x.Permission)
                .ThenInclude(x => x!.Details)
                    .ThenInclude(x => x.Function)
            .SingleOrDefaultAsync(x => x.Phone == phone, cancellationToken);

    private static string NormalizePhone(string phone) => phone.Trim();

    private static bool IsPhoneUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_user_Phone"
        };

    private static string HashToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

}
