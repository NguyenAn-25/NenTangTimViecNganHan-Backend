using System.ComponentModel.DataAnnotations;

namespace DotNetSkeleton.Shared.Validation.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class E164PhoneAttribute : ValidationAttribute
{
    public E164PhoneAttribute() : base(CommonValidationRules.PhoneE164Error)
    {
    }

    public override bool IsValid(object? value) =>
        value is null ||
        value is string text && (string.IsNullOrWhiteSpace(text) || CommonValidationRules.IsE164Phone(text));
}
