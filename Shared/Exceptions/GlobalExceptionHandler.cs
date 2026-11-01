using DotNetSkeleton.Shared.Api;
using Microsoft.AspNetCore.Diagnostics;

namespace DotNetSkeleton.Shared.Exceptions;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
            return false;

        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
            return true;

        var traceId = httpContext.TraceIdentifier;
        var statusCode = StatusCodes.Status500InternalServerError;
        var code = "internal_error";
        var message = "Đã xảy ra lỗi không mong muốn.";
        IReadOnlyDictionary<string, string[]>? errors = null;

        switch (exception)
        {
            case AppException applicationException:
                statusCode = applicationException.StatusCode;
                code = applicationException.Code;
                message = applicationException.Message;
                errors = applicationException.Errors;
                logger.LogWarning(
                    "Application error {ErrorCode} returned with status {StatusCode}. TraceId: {TraceId}",
                    code,
                    statusCode,
                    traceId);
                break;

            case FluentValidation.ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest;
                code = "validation_error";
                message = "Dữ liệu đầu vào không hợp lệ.";
                errors = validationException.Errors
                    .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "request" : error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).Distinct().ToArray());
                break;

            case System.ComponentModel.DataAnnotations.ValidationException validationException:
                statusCode = StatusCodes.Status400BadRequest;
                code = "validation_error";
                message = "Dữ liệu đầu vào không hợp lệ.";
                errors = new Dictionary<string, string[]>
                {
                    ["request"] = [validationException.Message]
                };
                break;

            default:
                logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);
                break;
        }

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(
            ApiResponse<object?>.Fail(message, code, traceId, errors),
            cancellationToken);
        return true;
    }
}
