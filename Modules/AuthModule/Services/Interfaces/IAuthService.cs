using DotNetSkeleton.Modules.AuthModule.DTOs.Requests;
using DotNetSkeleton.Modules.AuthModule.DTOs.Responses;

namespace DotNetSkeleton.Modules.AuthModule.Services.Interfaces;

public interface IAuthService
{
    Task<AuthOtpChallengeResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    Task<AuthResponseDto> VerifyRegistrationAsync(
        CompleteRegistrationRequestDto request,
        CancellationToken cancellationToken);
    Task<AuthOtpChallengeResponseDto> VerifyOtpAsync(
        Guid publicId,
        VerifyOtpRequestDto request,
        CancellationToken cancellationToken);
    Task ResendOtpAsync(Guid publicId, CancellationToken cancellationToken);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
    Task<AuthOtpChallengeResponseDto> RequestPasswordResetAsync(
        ForgotPasswordRequestDto request,
        CancellationToken cancellationToken);
    Task ResetPasswordAsync(ResetPasswordRequestDto request, CancellationToken cancellationToken);
    Task<AuthResponseDto> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken);
    Task LogoutAsync(int userId, int sessionId, CancellationToken cancellationToken);
    Task<UserResponseDto> GetProfileAsync(int userId, CancellationToken cancellationToken);
}
