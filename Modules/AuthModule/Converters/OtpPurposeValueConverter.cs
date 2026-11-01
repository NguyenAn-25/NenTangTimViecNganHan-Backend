using DotNetSkeleton.Modules.AuthModule.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetSkeleton.Modules.AuthModule.Converters;

public sealed class OtpPurposeValueConverter : ValueConverter<OtpPurpose, string>
{
    public OtpPurposeValueConverter()
        : base(
            purpose => ToStorageValue(purpose),
            value => FromStorageValue(value))
    {
    }

    private static string ToStorageValue(OtpPurpose purpose) =>
        purpose switch
        {
            OtpPurpose.Registration => "REGISTRATION",
            OtpPurpose.PasswordReset => "PASSWORD_RESET",
            _ => throw new InvalidOperationException($"Unsupported OTP purpose: {purpose}.")
        };

    private static OtpPurpose FromStorageValue(string value) =>
        value switch
        {
            "REGISTRATION" => OtpPurpose.Registration,
            "PASSWORD_RESET" => OtpPurpose.PasswordReset,
            _ => throw new InvalidOperationException($"Unsupported OTP purpose value: {value}.")
        };
}
