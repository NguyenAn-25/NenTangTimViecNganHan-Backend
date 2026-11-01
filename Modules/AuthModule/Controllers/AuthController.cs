using System.Security.Claims;
using DotNetSkeleton.Modules.AuthModule.DTOs.Requests;
using DotNetSkeleton.Modules.AuthModule.DTOs.Responses;
using DotNetSkeleton.Modules.AuthModule.Exceptions;
using DotNetSkeleton.Modules.AuthModule.Services.Interfaces;
using DotNetSkeleton.Shared.Api;
using DotNetSkeleton.Shared.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DotNetSkeleton.Modules.AuthModule.Controllers;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController(IAuthService authService) : ApiControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType<ApiResponse<AuthOtpChallengeResponseDto>>(StatusCodes.Status202Accepted)]
    public async Task<ActionResult<ApiResponse<AuthOtpChallengeResponseDto>>> Register(
        RegisterRequestDto request,
        CancellationToken cancellationToken) =>
        AcceptedResponse(
            await authService.RegisterAsync(request, cancellationToken),
            "Nếu số điện thoại có thể đăng ký, mã xác thực sẽ được gửi đến số đó.");

    [HttpPost("otp/verify/{publicId:guid}")]
    [ProducesResponseType<ApiResponse<AuthOtpChallengeResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthOtpChallengeResponseDto>>> VerifyOtp(
        Guid publicId,
        VerifyOtpRequestDto request,
        CancellationToken cancellationToken) =>
        OkResponse(await authService.VerifyOtpAsync(publicId, request, cancellationToken));

    [HttpPost("otp/resend/{publicId:guid}")]
    [ProducesResponseType<ApiResponse<object>>(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ResendOtp(Guid publicId, CancellationToken cancellationToken)
    {
        await authService.ResendOtpAsync(publicId, cancellationToken);
        return AcceptedResponse("Nếu yêu cầu còn hiệu lực, mã xác thực sẽ được gửi lại.");
    }

    [HttpPost("register/complete")]
    [ProducesResponseType<ApiResponse<AuthResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> CompleteRegistration(
        CompleteRegistrationRequestDto request,
        CancellationToken cancellationToken) =>
        OkResponse(await authService.VerifyRegistrationAsync(request, cancellationToken));

    [HttpPost("login")]
    [ProducesResponseType<ApiResponse<AuthResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        LoginRequestDto request,
        CancellationToken cancellationToken) =>
        OkResponse(await authService.LoginAsync(request, cancellationToken));

    [HttpPost("password/forgot")]
    [ProducesResponseType<ApiResponse<AuthOtpChallengeResponseDto>>(StatusCodes.Status202Accepted)]
    public async Task<ActionResult<ApiResponse<AuthOtpChallengeResponseDto>>> ForgotPassword(
        ForgotPasswordRequestDto request,
        CancellationToken cancellationToken) =>
        AcceptedResponse(
            await authService.RequestPasswordResetAsync(request, cancellationToken),
            "Nếu tài khoản tồn tại, mã đặt lại mật khẩu sẽ được gửi đến số điện thoại.");

    [HttpPost("password/reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        await authService.ResetPasswordAsync(request, cancellationToken);
        return NoContentResponse();
    }

    [HttpPost("refresh")]
    [ProducesResponseType<ApiResponse<AuthResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Refresh(
        RefreshTokenRequestDto request,
        CancellationToken cancellationToken) =>
        OkResponse(await authService.RefreshAsync(request, cancellationToken));

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ||
            !int.TryParse(User.FindFirstValue("session_id"), out var sessionId))
        {
            throw new AuthSessionClaimsInvalidException();
        }

        await authService.LogoutAsync(userId, sessionId, cancellationToken);
        return NoContentResponse();
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType<ApiResponse<UserResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserResponseDto>>> Me(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            throw new AuthUserClaimsInvalidException();

        return OkResponse(await authService.GetProfileAsync(userId, cancellationToken));
    }
}
