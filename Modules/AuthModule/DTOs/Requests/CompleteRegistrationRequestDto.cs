using System.ComponentModel.DataAnnotations;
using DotNetSkeleton.Shared.Validation.Attributes;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Requests;

public sealed class CompleteRegistrationRequestDto
{
    [Required(ErrorMessage = "Mã publicId là bắt buộc!")]
    public Guid PublicId { get; init; }

    [Required(ErrorMessage = "Mật khẩu là bắt buộc!")]
    [PasswordStrength]
    public string Password { get; init; } = string.Empty;
}
