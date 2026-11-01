using System.ComponentModel.DataAnnotations;

namespace DotNetSkeleton.Shared.Validation.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class PasswordStrengthAttribute : ValidationAttribute
{
    public PasswordStrengthAttribute() : base(CommonValidationRules.PasswordStrengthError)
    {
    }

    public override bool IsValid(object? value) =>
        value is null ||
        value is string text &&
        (string.IsNullOrWhiteSpace(text) || CommonValidationRules.IsPasswordStrengthValid(text));
}
