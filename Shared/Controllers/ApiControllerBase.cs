using DotNetSkeleton.Shared.Api;
using Microsoft.AspNetCore.Mvc;

namespace DotNetSkeleton.Shared.Controllers;

/// <summary>
/// Common response helpers for API controllers in every module.
/// </summary>
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string? message = null) =>
        Ok(ApiResponse<T>.Succeed(data, message, HttpContext.TraceIdentifier));

    protected IActionResult AcceptedResponse(string message) =>
        Accepted(ApiResponse<object?>.Succeed(null, message, HttpContext.TraceIdentifier));

    protected ActionResult<ApiResponse<T>> AcceptedResponse<T>(T data, string? message = null) =>
        Accepted(ApiResponse<T>.Succeed(data, message, HttpContext.TraceIdentifier));

    protected ActionResult<ApiResponse<T>> AcceptedAtActionResponse<T>(
        string actionName,
        object? routeValues,
        T data,
        string? message = null) =>
        AcceptedAtAction(
            actionName,
            routeValues,
            ApiResponse<T>.Succeed(data, message, HttpContext.TraceIdentifier));

    protected ActionResult<ApiResponse<T>> CreatedAtActionResponse<T>(
        string actionName,
        object? routeValues,
        T data,
        string? message = null) =>
        CreatedAtAction(
            actionName,
            routeValues,
            ApiResponse<T>.Succeed(data, message, HttpContext.TraceIdentifier));

    protected IActionResult NoContentResponse() => NoContent();

    protected IActionResult BadRequestResponse(string message, string code = "bad_request") =>
        ErrorResponse(StatusCodes.Status400BadRequest, message, code);

    protected IActionResult UnauthorizedResponse(string message = "Authentication is required.") =>
        ErrorResponse(StatusCodes.Status401Unauthorized, message, "unauthorized");

    protected IActionResult ForbiddenResponse(string message = "You do not have permission to perform this action.") =>
        ErrorResponse(StatusCodes.Status403Forbidden, message, "forbidden");

    protected ActionResult<ApiResponse<T>> UnauthorizedResponse<T>(string message = "Authentication is required.") =>
        Unauthorized(ApiResponse<T>.Fail(message, "unauthorized", HttpContext.TraceIdentifier));

    protected IActionResult NotFoundResponse(string message = "The requested resource was not found.") =>
        ErrorResponse(StatusCodes.Status404NotFound, message, "not_found");

    protected IActionResult ConflictResponse(string message, string code = "conflict") =>
        ErrorResponse(StatusCodes.Status409Conflict, message, code);

    private ObjectResult ErrorResponse(int statusCode, string message, string code)
    {
        var response = ApiResponse<object?>.Fail(message, code, HttpContext.TraceIdentifier);
        return StatusCode(statusCode, response);
    }
}
