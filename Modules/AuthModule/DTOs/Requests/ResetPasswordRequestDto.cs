using System.ComponentModel.DataAnnotations;
using DotNetSkeleton.Shared.Validation.Attributes;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Requests;

public sealed class ResetPasswordRequestDto
{
    [Required(ErrorMessage = "Mã publicId là bắt buộc!")]
    public Guid PublicId { get; init; }

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc!")]
    [PasswordStrength]
    public string NewPassword { get; init; } = string.Empty;
}
