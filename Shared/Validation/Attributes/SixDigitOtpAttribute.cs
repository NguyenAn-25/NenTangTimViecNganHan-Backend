using System.ComponentModel.DataAnnotations;

namespace DotNetSkeleton.Shared.Validation.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class SixDigitOtpAttribute : ValidationAttribute
{
    public SixDigitOtpAttribute() : base(CommonValidationRules.SixDigitOtpError)
    {
    }

    public override bool IsValid(object? value) =>
        value is null ||
        value is string text && (string.IsNullOrWhiteSpace(text) || CommonValidationRules.IsSixDigitOtp(text));
}
