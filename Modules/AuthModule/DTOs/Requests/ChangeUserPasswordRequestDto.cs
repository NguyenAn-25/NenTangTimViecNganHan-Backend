using System.ComponentModel.DataAnnotations;
using DotNetSkeleton.Shared.Validation.Attributes;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Requests;

public sealed class ChangeUserPasswordRequestDto
{
    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc!")]
    [PasswordStrength]
    public string NewPassword { get; init; } = string.Empty;
}
