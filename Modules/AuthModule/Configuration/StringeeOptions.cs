namespace DotNetSkeleton.Modules.AuthModule.Configuration;

public sealed class StringeeOptions
{
    public const string SectionName = "Stringee";

    public string ApiKeySid { get; init; } = string.Empty;
    public string ApiKeySecret { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
}
