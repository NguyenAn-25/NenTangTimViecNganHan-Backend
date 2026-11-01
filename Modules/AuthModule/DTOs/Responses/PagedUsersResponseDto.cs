namespace DotNetSkeleton.Modules.AuthModule.DTOs.Responses;

public sealed record PagedUsersResponseDto(
    IReadOnlyList<UserSummaryResponseDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
