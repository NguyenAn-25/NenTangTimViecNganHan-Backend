using System.ComponentModel.DataAnnotations;
using DotNetSkeleton.Shared.Validation.Attributes;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Requests;

public sealed class ForgotPasswordRequestDto
{
    [Required(ErrorMessage = "Số điện thoại là bắt buộc!")]
    [E164Phone]
    public string Phone { get; init; } = string.Empty;
}
