using DotNetSkeleton.Modules.AuthModule.DTOs.Requests;
using DotNetSkeleton.Modules.AuthModule.DTOs.Responses;

namespace DotNetSkeleton.Modules.AuthModule.Services.Interfaces;

public interface IUsersService
{
    Task<PagedUsersResponseDto> ListAsync(ListUsersRequestDto request, CancellationToken cancellationToken);
    Task<UserDetailResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<UserDetailResponseDto> CreateAsync(CreateUserRequestDto request, CancellationToken cancellationToken);
    Task<UserDetailResponseDto> UpdateAsync(int id, UpdateUserRequestDto request, CancellationToken cancellationToken);
    Task UpdatePasswordAsync(int id, ChangeUserPasswordRequestDto request, CancellationToken cancellationToken);
    Task<UserDetailResponseDto> UpdateStatusAsync(int id, UpdateUserStatusRequestDto request, CancellationToken cancellationToken);
}
