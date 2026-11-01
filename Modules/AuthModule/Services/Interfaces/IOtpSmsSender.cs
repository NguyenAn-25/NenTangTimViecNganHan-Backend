namespace DotNetSkeleton.Modules.AuthModule.Services.Interfaces;

/// <summary>
/// SMS transport boundary. Production uses the third-party Stringee API; Development can use a local logger.
/// </summary>
public interface IOtpSmsSender
{
    Task SendAsync(string phone, string code, CancellationToken cancellationToken);
}
