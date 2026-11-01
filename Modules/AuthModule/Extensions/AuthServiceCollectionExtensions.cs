using DotNetSkeleton.Modules.AuthModule.Authorization;
using DotNetSkeleton.Modules.AuthModule.Converters;
using DotNetSkeleton.Modules.AuthModule.Entities;
using DotNetSkeleton.Modules.AuthModule.Services;
using DotNetSkeleton.Modules.AuthModule.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DotNetSkeleton.Modules.AuthModule.Extensions;

public static class AuthServiceCollectionExtensions
{
    public static IServiceCollection AddAuthModule(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.AddJwtAuthentication(configuration);
        services.AddAuthorization();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddScoped<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();
        services.AddScoped<AuthTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsersService, UserService>();

        services.AddOtpSms(configuration, environment);
        services.ConfigureHttpJsonOptions(options => AddEnumJsonConverters(options.SerializerOptions));
        services.AddControllers()
            .AddJsonOptions(options => AddEnumJsonConverters(options.JsonSerializerOptions));
        services.AddAuthRateLimiting();

        return services;
    }

    private static void AddEnumJsonConverters(System.Text.Json.JsonSerializerOptions options)
    {
        options.Converters.Add(new OtpPurposeJsonConverter());
        options.Converters.Add(new PermissionStatusJsonConverter());
        options.Converters.Add(new UserRoleJsonConverter());
    }
}
