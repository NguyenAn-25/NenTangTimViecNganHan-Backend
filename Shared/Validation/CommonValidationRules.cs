namespace DotNetSkeleton.Shared.Validation;

/// <summary>
/// Shared validation predicates for request annotations and FluentValidation rules.
/// </summary>
public static class CommonValidationRules
{
    public const string PhoneE164Error = "Số điện thoại phải theo định dạng E.164, ví dụ +84901234567.";
    public const string SixDigitOtpError = "Mã OTP phải gồm đúng 6 chữ số.";
    public const string PasswordStrengthError =
        "Mật khẩu phải dài từ 8 đến 128 ký tự, có ít nhất một chữ in hoa, một chữ số và một ký tự đặc biệt.";

    public static bool IsE164Phone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length is < 9 or > 16 || value[0] != '+')
            return false;

        if (value[1] is < '1' or > '9')
            return false;

        for (var index = 2; index < value.Length; index++)
        {
            if (value[index] is < '0' or > '9')
                return false;
        }

        return true;
    }

    public static bool IsSixDigitOtp(string? value) =>
        value is { Length: 6 } && value.All(character => character is >= '0' and <= '9');

    public static bool IsPasswordStrengthValid(string? value) =>
        value is { Length: >= 8 and <= 128 } &&
        value.Any(char.IsUpper) &&
        value.Any(char.IsDigit) &&
        value.Any(character => char.IsPunctuation(character) || char.IsSymbol(character));

    public static bool IsRequiredText(string? value) =>
        !string.IsNullOrWhiteSpace(value);

    public static bool HasLengthBetween(string? value, int minimumLength, int maximumLength) =>
        value is not null && value.Length >= minimumLength && value.Length <= maximumLength;
}
