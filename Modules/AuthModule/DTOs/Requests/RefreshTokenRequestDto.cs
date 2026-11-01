using System.ComponentModel.DataAnnotations;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Requests;

public sealed class RefreshTokenRequestDto
{
    [Required(ErrorMessage = "Refresh token là bắt buộc!")]
    public string RefreshToken { get; init; } = string.Empty;
}
