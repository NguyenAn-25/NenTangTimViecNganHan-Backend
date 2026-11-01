using System.Security.Claims;
using System.Text;
using DotNetSkeleton.Modules.AuthModule.Configuration;
using DotNetSkeleton.Shared.Api;
using DotNetSkeleton.Shared.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DotNetSkeleton.Modules.AuthModule.Extensions;

internal static class JwtAuthenticationServiceCollectionExtensions
{
    internal static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsJsonAsync(
                            ApiResponse<object?>.Fail(
                                "Token không hợp lệ hoặc chưa được cung cấp.",
                                "unauthorized",
                                context.HttpContext.TraceIdentifier));
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(
                            ApiResponse<object?>.Fail(
                                "Bạn không có quyền thực hiện thao tác này.",
                                "forbidden",
                                context.HttpContext.TraceIdentifier));
                    },
                    OnTokenValidated = async context =>
                    {
                        var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var sessionIdValue = context.Principal?.FindFirstValue("session_id");
                        if (!int.TryParse(userIdValue, out var userId) ||
                            !int.TryParse(sessionIdValue, out var sessionId))
                        {
                            context.Fail("Invalid session claims.");
                            return;
                        }

                        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                        var now = DateTimeOffset.UtcNow;
                        var sessionIsActive = await dbContext.AuthSessions
                            .AsNoTracking()
                            .AnyAsync(x => x.Id == sessionId && x.UserId == userId &&
                                           x.RevokedAt == null && x.ExpiredAt > now &&
                                           x.User.IsActive && x.User.IsVerified,
                                context.HttpContext.RequestAborted);
                        if (!sessionIsActive)
                            context.Fail("Session is revoked, expired, or inactive.");
                    }
                };
            });

        return services;
    }
}
