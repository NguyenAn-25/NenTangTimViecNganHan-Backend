using DotNetSkeleton.Modules.AuthModule.Enums;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Responses;

public sealed record UserResponseDto(
    int Id,
    Guid PublicId,
    string Phone,
    bool IsActive,
    bool IsVerified,
    DateTimeOffset? VerifiedAt,
    DateTimeOffset CreatedAt,
    UserRole Role);
