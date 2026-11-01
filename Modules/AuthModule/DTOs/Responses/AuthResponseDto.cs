namespace DotNetSkeleton.Modules.AuthModule.DTOs.Responses;

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    UserResponseDto User);
