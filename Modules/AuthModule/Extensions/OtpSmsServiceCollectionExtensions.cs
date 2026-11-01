using DotNetSkeleton.Modules.AuthModule.Configuration;
using DotNetSkeleton.Modules.AuthModule.Services;
using DotNetSkeleton.Modules.AuthModule.Services.Interfaces;

namespace DotNetSkeleton.Modules.AuthModule.Extensions;

internal static class OtpSmsServiceCollectionExtensions
{
    internal static IServiceCollection AddOtpSms(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var stringee = configuration.GetSection(StringeeOptions.SectionName).Get<StringeeOptions>() ?? new StringeeOptions();
        var stringeeConfigured = !string.IsNullOrWhiteSpace(stringee.ApiKeySid) &&
                                 !string.IsNullOrWhiteSpace(stringee.ApiKeySecret) &&
                                 !string.IsNullOrWhiteSpace(stringee.BrandName);

        services.AddOptions<StringeeOptions>().BindConfiguration(StringeeOptions.SectionName);
        if (environment.IsDevelopment())
        {
            // Temporarily bypass the third-party Stringee API in Development, even when credentials exist.
            // The local sender logs the random OTP so the auth flow remains testable without sending SMS.
            services.AddScoped<IOtpSmsSender, DevelopmentOtpSmsSender>();
        }
        else if (stringeeConfigured)
        {
            // This is the non-Development integration point for the third-party Stringee SMS API.
            services.AddHttpClient<StringeeOtpSmsSender>(client =>
                client.Timeout = TimeSpan.FromSeconds(20));
            services.AddScoped<IOtpSmsSender>(provider =>
                provider.GetRequiredService<StringeeOtpSmsSender>());
        }
        else
        {
            throw new InvalidOperationException(
                "Production requires Stringee:ApiKeySid, Stringee:ApiKeySecret and Stringee:BrandName configuration.");
        }

        return services;
    }
}
