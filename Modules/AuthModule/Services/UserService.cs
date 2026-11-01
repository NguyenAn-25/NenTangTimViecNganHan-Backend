using DotNetSkeleton.Modules.AuthModule.DTOs.Requests;
using DotNetSkeleton.Modules.AuthModule.DTOs.Responses;
using DotNetSkeleton.Modules.AuthModule.Entities;
using DotNetSkeleton.Modules.AuthModule.Enums;
using DotNetSkeleton.Modules.AuthModule.Exceptions;
using DotNetSkeleton.Modules.AuthModule.Mappings;
using DotNetSkeleton.Modules.AuthModule.Services.Interfaces;
using DotNetSkeleton.Shared.Persistence;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DotNetSkeleton.Modules.AuthModule.Services;

public sealed class UserService(
    AppDbContext context,
    IPasswordHasher<UserEntity> passwordHasher) : IUsersService
{
    public async Task<PagedUsersResponseDto> ListAsync(
        ListUsersRequestDto request,
        CancellationToken cancellationToken)
    {
        var query = context.Users.AsNoTracking();
        var search = request.Search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(user => user.Phone.Contains(search));
        if (request.Role is { } role)
            query = query.Where(user => user.Role == role);
        if (request.IsActive is { } isActive)
            query = query.Where(user => user.IsActive == isActive);
        if (request.IsVerified is { } isVerified)
            query = query.Where(user => user.IsVerified == isVerified);
        if (request.PermissionId is { } permissionId)
            query = query.Where(user => user.PermissionId == permissionId);
        if (request.CreatedFrom is { } createdFrom)
            query = query.Where(user => user.CreatedAt >= createdFrom);
        if (request.CreatedTo is { } createdTo)
            query = query.Where(user => user.CreatedAt <= createdTo);

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = ((long)request.Page - 1) * request.PageSize;
        if (offset > int.MaxValue)
            return new PagedUsersResponseDto([], request.Page, request.PageSize, totalCount);

        var users = await query
            .OrderByDescending(user => user.CreatedAt)
            .ThenBy(user => user.Id)
            .Skip((int)offset)
            .Take(request.PageSize)
            .ProjectToType<UserSummaryResponseDto>(AuthMappingConfiguration.Config)
            .ToListAsync(cancellationToken);

        return new PagedUsersResponseDto(users, request.Page, request.PageSize, totalCount);
    }

    public async Task<UserDetailResponseDto> GetByIdAsync(
        int id,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .ProjectToType<UserDetailResponseDto>(AuthMappingConfiguration.Config)
            .SingleOrDefaultAsync(cancellationToken);

        return user ?? throw new AuthUserNotFoundException();
    }

    public async Task<UserDetailResponseDto> CreateAsync(
        CreateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var phone = request.Phone.Trim();
        if (await context.Users.AnyAsync(user => user.Phone == phone, cancellationToken))
            throw new AuthUserPhoneAlreadyInUseException();

        var permission = await ResolvePermissionAsync(request.PermissionId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var user = new UserEntity
        {
            Phone = phone,
            IsActive = true,
            IsVerified = true,
            VerifiedAt = now,
            CreatedAt = now,
            Role = UserRole.Admin,
            PermissionId = permission.Id
        };
        user.Password = passwordHasher.HashPassword(user, request.Password);

        context.Users.Add(user);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsPhoneUniqueViolation(exception))
        {
            throw new AuthUserPhoneAlreadyInUseException();
        }

        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<UserDetailResponseDto> UpdateAsync(
        int id,
        UpdateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(
            user => user.Id == id,
            cancellationToken);
        if (user is null)
            throw new AuthUserNotFoundException();
        if (user.Role != UserRole.Admin)
            throw new AuthUserRoleCannotHavePermissionException();

        var permission = await ResolvePermissionAsync(request.PermissionId, cancellationToken);
        if (user.PermissionId == permission.Id)
            return await GetByIdAsync(id, cancellationToken);

        await using (var transaction = await context.Database.BeginTransactionAsync(cancellationToken))
        {
            user.PermissionId = permission.Id;
            await RevokeActiveSessionsAsync(user.Id, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task UpdatePasswordAsync(
        int id,
        ChangeUserPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(
            user => user.Id == id,
            cancellationToken);
        if (user is null)
            throw new AuthUserNotFoundException();

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        user.Password = passwordHasher.HashPassword(user, request.NewPassword);
        await RevokeActiveSessionsAsync(user.Id, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<UserDetailResponseDto> UpdateStatusAsync(
        int id,
        UpdateUserStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await context.Users.SingleOrDefaultAsync(
            user => user.Id == id,
            cancellationToken);
        if (user is null)
            throw new AuthUserNotFoundException();

        var newStatus = request.IsActive!.Value;
        if (newStatus && !user.IsVerified)
            throw new AuthUserCannotBeActivatedBeforeVerificationException();
        if (user.IsActive == newStatus)
            return await GetByIdAsync(id, cancellationToken);

        await using (var transaction = await context.Database.BeginTransactionAsync(cancellationToken))
        {
            user.IsActive = newStatus;
            if (!newStatus)
                await RevokeActiveSessionsAsync(user.Id, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        return await GetByIdAsync(id, cancellationToken);
    }

    private async Task<PermissionEntity> ResolvePermissionAsync(
        int permissionId,
        CancellationToken cancellationToken)
    {
        var permission = await context.Permissions.SingleOrDefaultAsync(
            permission => permission.Id == permissionId && permission.Status == PermissionStatus.Active,
            cancellationToken);

        return permission ?? throw new AuthUserPermissionUnavailableException();
    }

    private async Task RevokeActiveSessionsAsync(int userId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        await context.AuthSessions
            .Where(session => session.UserId == userId && session.RevokedAt == null)
            .ExecuteUpdateAsync(update => update.SetProperty(session => session.RevokedAt, now), cancellationToken);
    }

    private static bool IsPhoneUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_user_Phone"
        };
}
