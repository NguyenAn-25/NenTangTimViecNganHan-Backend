using System.ComponentModel.DataAnnotations;
using DotNetSkeleton.Shared.Validation.Attributes;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Requests;

public sealed class LoginRequestDto
{
    [Required(ErrorMessage = "Số điện thoại là bắt buộc!")]
    [E164Phone]
    public string Phone { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc!")]
    [MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}
