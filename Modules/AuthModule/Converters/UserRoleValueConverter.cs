using DotNetSkeleton.Modules.AuthModule.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetSkeleton.Modules.AuthModule.Converters;

public sealed class UserRoleValueConverter : ValueConverter<UserRole, string>
{
    public UserRoleValueConverter()
        : base(
            role => ToStorageValue(role),
            value => FromStorageValue(value))
    {
    }

    private static string ToStorageValue(UserRole role) =>
        role switch
        {
            UserRole.User => "USER",
            UserRole.Admin => "ADMIN",
            _ => throw new InvalidOperationException($"Unsupported user role: {role}.")
        };

    private static UserRole FromStorageValue(string value) =>
        value switch
        {
            "USER" => UserRole.User,
            "ADMIN" => UserRole.Admin,
            _ => throw new InvalidOperationException($"Unsupported user role value: {value}.")
        };
}
