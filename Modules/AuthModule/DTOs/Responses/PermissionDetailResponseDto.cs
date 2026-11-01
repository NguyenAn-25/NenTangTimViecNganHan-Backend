using DotNetSkeleton.Modules.AuthModule.Enums;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Responses;

public sealed record PermissionDetailResponseDto(
    int Id,
    string Name,
    string? Description,
    PermissionStatus Status);
