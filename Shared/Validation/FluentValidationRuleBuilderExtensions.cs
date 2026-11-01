using FluentValidation;

namespace DotNetSkeleton.Shared.Validation;

public static class FluentValidationRuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string> MustBeE164Phone<T>(
        this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .Must(value => CommonValidationRules.IsE164Phone(value))
            .WithMessage(CommonValidationRules.PhoneE164Error);

    public static IRuleBuilderOptions<T, string> MustBeSixDigitOtp<T>(
        this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .Must(value => CommonValidationRules.IsSixDigitOtp(value))
            .WithMessage(CommonValidationRules.SixDigitOtpError);

    public static IRuleBuilderOptions<T, string> MustHaveStrongPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .Must(value => CommonValidationRules.IsPasswordStrengthValid(value))
            .WithMessage(CommonValidationRules.PasswordStrengthError);

    public static IRuleBuilderOptions<T, string> MustBeRequiredText<T>(
        this IRuleBuilder<T, string> ruleBuilder) =>
        ruleBuilder
            .Must(value => CommonValidationRules.IsRequiredText(value))
            .WithMessage("Giá trị không được để trống.");

    public static IRuleBuilderOptions<T, string> MustHaveLengthBetween<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        int minimumLength,
        int maximumLength) =>
        ruleBuilder
            .Must(value => CommonValidationRules.HasLengthBetween(value, minimumLength, maximumLength))
            .WithMessage($"Giá trị phải có từ {minimumLength} đến {maximumLength} ký tự.");
}
