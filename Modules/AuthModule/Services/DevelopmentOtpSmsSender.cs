using DotNetSkeleton.Modules.AuthModule.Services.Interfaces;

namespace DotNetSkeleton.Modules.AuthModule.Services;

/// <summary>Local-only OTP transport. Never register this implementation in Production.</summary>
public sealed class DevelopmentOtpSmsSender(ILogger<DevelopmentOtpSmsSender> logger) : IOtpSmsSender
{
    public Task SendAsync(string phone, string code, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogWarning("Development OTP for {Phone}: {Code}", phone, code);
        return Task.CompletedTask;
    }
}
