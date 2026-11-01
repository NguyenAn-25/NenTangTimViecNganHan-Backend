using DotNetSkeleton.Modules.AuthModule.DTOs.Requests;
using DotNetSkeleton.Modules.AuthModule.DTOs.Responses;
using DotNetSkeleton.Modules.AuthModule.Services.Interfaces;
using DotNetSkeleton.Shared.Api;
using DotNetSkeleton.Shared.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotNetSkeleton.Modules.AuthModule.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController(IUsersService usersService) : ApiControllerBase
{
    [Authorize(Policy = "permission:users:read")]
    [HttpGet]
    [ProducesResponseType<ApiResponse<PagedUsersResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedUsersResponseDto>>> List(
        [FromQuery] ListUsersRequestDto request,
        CancellationToken cancellationToken) =>
        OkResponse(await usersService.ListAsync(request, cancellationToken));

    [Authorize(Policy = "permission:users:read")]
    [HttpGet("{id:int}")]
    [ProducesResponseType<ApiResponse<UserDetailResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserDetailResponseDto>>> GetById(
        int id,
        CancellationToken cancellationToken) =>
        OkResponse(await usersService.GetByIdAsync(id, cancellationToken));

    [Authorize(Policy = "permission:users:create")]
    [HttpPost]
    [ProducesResponseType<ApiResponse<UserDetailResponseDto>>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<UserDetailResponseDto>>> Create(
        CreateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var user = await usersService.CreateAsync(request, cancellationToken);
        return CreatedAtActionResponse(nameof(GetById), new { id = user.Id }, user);
    }

    [Authorize(Policy = "permission:users:update")]
    [HttpPut("{id:int}")]
    [ProducesResponseType<ApiResponse<UserDetailResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserDetailResponseDto>>> Update(
        int id,
        UpdateUserRequestDto request,
        CancellationToken cancellationToken) =>
        OkResponse(await usersService.UpdateAsync(id, request, cancellationToken));

    [Authorize(Policy = "permission:users:update")]
    [HttpPut("{id:int}/password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePassword(
        int id,
        ChangeUserPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        await usersService.UpdatePasswordAsync(id, request, cancellationToken);
        return NoContentResponse();
    }

    [Authorize(Policy = "permission:users:update")]
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType<ApiResponse<UserDetailResponseDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<UserDetailResponseDto>>> UpdateStatus(
        int id,
        UpdateUserStatusRequestDto request,
        CancellationToken cancellationToken) =>
        OkResponse(await usersService.UpdateStatusAsync(id, request, cancellationToken));
}
