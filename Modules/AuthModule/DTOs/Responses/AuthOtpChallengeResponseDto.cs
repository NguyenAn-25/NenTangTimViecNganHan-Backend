using DotNetSkeleton.Modules.AuthModule.Enums;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Responses;

public sealed record AuthOtpChallengeResponseDto(
    Guid PublicId,
    OtpPurpose Purpose,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset ResendAvailableAt,
    DateTimeOffset? VerifiedAt);
