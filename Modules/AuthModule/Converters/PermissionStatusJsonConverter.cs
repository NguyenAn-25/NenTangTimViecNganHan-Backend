using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetSkeleton.Modules.AuthModule.Enums;

namespace DotNetSkeleton.Modules.AuthModule.Converters;

public sealed class PermissionStatusJsonConverter : JsonConverter<PermissionStatus>
{
    public override PermissionStatus Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Giá trị enum phải là chuỗi.");

        return reader.GetString() switch
        {
            "Hoạt động" => PermissionStatus.Active,
            "Không hoạt động" => PermissionStatus.Inactive,
            _ => throw new JsonException("Trạng thái permission không hợp lệ.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        PermissionStatus value,
        JsonSerializerOptions options) =>
        writer.WriteStringValue(value switch
        {
            PermissionStatus.Active => "Hoạt động",
            PermissionStatus.Inactive => "Không hoạt động",
            _ => throw new JsonException($"Trạng thái permission không được hỗ trợ: {value}.")
        });
}
