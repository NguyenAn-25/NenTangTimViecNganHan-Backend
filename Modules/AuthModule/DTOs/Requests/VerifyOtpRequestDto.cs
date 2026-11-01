using System.ComponentModel.DataAnnotations;
using DotNetSkeleton.Shared.Validation.Attributes;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Requests;

public sealed class VerifyOtpRequestDto
{
    [Required(ErrorMessage = "Mã OTP là bắt buộc!")]
    [SixDigitOtp]
    public string Code { get; init; } = string.Empty;
}
