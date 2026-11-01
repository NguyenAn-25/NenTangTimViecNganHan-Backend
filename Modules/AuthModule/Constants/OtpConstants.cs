namespace DotNetSkeleton.Modules.AuthModule.Constants;

public static class OtpConstants
{
    public const string DevelopmentOtpCode = "012345";
    public const int MaxFailedAttempts = 5;
    public const int ResendCooldownSeconds = 30;
}
