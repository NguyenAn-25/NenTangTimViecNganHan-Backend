using DotNetSkeleton.Modules.AuthModule.Enums;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Responses;

public sealed record UserDetailResponseDto(
    int Id,
    Guid PublicId,
    string Phone,
    bool IsActive,
    bool IsVerified,
    DateTimeOffset? VerifiedAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastActiveAt,
    UserRole Role,
    PermissionDetailResponseDto? Permission);
