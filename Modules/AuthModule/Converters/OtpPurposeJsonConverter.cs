using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetSkeleton.Modules.AuthModule.Enums;

namespace DotNetSkeleton.Modules.AuthModule.Converters;

public sealed class OtpPurposeJsonConverter : JsonConverter<OtpPurpose>
{
    public override OtpPurpose Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Giá trị enum phải là chuỗi.");

        return reader.GetString() switch
        {
            "Đăng ký" => OtpPurpose.Registration,
            "Đặt lại mật khẩu" => OtpPurpose.PasswordReset,
            _ => throw new JsonException("Giá trị mục đích OTP không hợp lệ.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        OtpPurpose value,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(value switch
        {
            OtpPurpose.Registration => "Đăng ký",
            OtpPurpose.PasswordReset => "Đặt lại mật khẩu",
            _ => throw new JsonException($"Mục đích OTP không được hỗ trợ: {value}.")
        });
}
