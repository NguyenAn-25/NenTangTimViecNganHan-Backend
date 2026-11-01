using DotNetSkeleton.Modules.AuthModule.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetSkeleton.Modules.AuthModule.Converters;

public sealed class PermissionStatusValueConverter : ValueConverter<PermissionStatus, string>
{
    public PermissionStatusValueConverter()
        : base(
            status => ToStorageValue(status),
            value => FromStorageValue(value))
    {
    }

    private static string ToStorageValue(PermissionStatus status) =>
        status switch
        {
            PermissionStatus.Active => "ACTIVE",
            PermissionStatus.Inactive => "INACTIVE",
            _ => throw new InvalidOperationException($"Unsupported permission status: {status}.")
        };

    private static PermissionStatus FromStorageValue(string value) =>
        value switch
        {
            "ACTIVE" => PermissionStatus.Active,
            "INACTIVE" => PermissionStatus.Inactive,
            _ => throw new InvalidOperationException($"Unsupported permission status value: {value}.")
        };
}
