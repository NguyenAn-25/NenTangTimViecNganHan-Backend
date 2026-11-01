using System.ComponentModel.DataAnnotations;
using DotNetSkeleton.Shared.Validation.Attributes;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Requests;

public sealed class CreateUserRequestDto
{
    [Required(ErrorMessage = "Số điện thoại là bắt buộc!")]
    [E164Phone]
    public string Phone { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc!")]
    [PasswordStrength]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Quyền hạn là bắt buộc!")]
    [Range(1, int.MaxValue, ErrorMessage = "Quyền hạn phải là số nguyên dương bắt đầu từ 1!")]
    public int PermissionId { get; init; }
}
