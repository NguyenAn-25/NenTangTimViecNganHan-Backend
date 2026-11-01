using DotNetSkeleton.Modules.AuthModule.Enums;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Responses;

public sealed record UserSummaryResponseDto(
    int Id,
    Guid PublicId,
    string Phone,
    bool IsActive,
    bool IsVerified,
    DateTimeOffset CreatedAt,
    UserRole Role,
    PermissionSummaryResponseDto? Permission);
