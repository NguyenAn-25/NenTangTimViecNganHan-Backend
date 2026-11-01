using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetSkeleton.Modules.AuthModule.Enums;

namespace DotNetSkeleton.Modules.AuthModule.Converters;

public sealed class UserRoleJsonConverter : JsonConverter<UserRole>
{
    public override UserRole Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Giá trị enum phải là chuỗi.");

        return reader.GetString() switch
        {
            "Người dùng" or "USER" => UserRole.User,
            "Quản trị viên" or "ADMIN" => UserRole.Admin,
            _ => throw new JsonException("Role người dùng không hợp lệ.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        UserRole value,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(value switch
        {
            UserRole.User => "Người dùng",
            UserRole.Admin => "Quản trị viên",
            _ => throw new JsonException($"Role không được hỗ trợ: {value}.")
        });
}
