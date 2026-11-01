using System.Threading.RateLimiting;
using DotNetSkeleton.Shared.Api;
using Microsoft.AspNetCore.RateLimiting;

namespace DotNetSkeleton.Modules.AuthModule.Extensions;

internal static class AuthRateLimitingServiceCollectionExtensions
{
    internal static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await httpContext.Response.WriteAsJsonAsync(
                    ApiResponse<object?>.Fail(
                        "Bạn đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau.",
                        "rate_limit_exceeded",
                        httpContext.TraceIdentifier),
                    cancellationToken);
            };
            options.AddPolicy("auth", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 6,
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });

        return services;
    }
}
