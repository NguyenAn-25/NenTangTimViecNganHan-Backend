namespace DotNetSkeleton.Shared.Api;

/// <summary>
/// Consistent JSON envelope for successful and failed API responses.
/// HTTP status remains the source of truth for the outcome.
/// </summary>
public sealed record ApiResponse<T>(
    bool Success,
    string? Message,
    T? Data,
    IReadOnlyDictionary<string, string[]>? Errors,
    string? Code,
    string? TraceId)
{
    public static ApiResponse<T> Succeed(T? data, string? message = null, string? traceId = null) =>
        new(true, message, data, null, null, traceId);

    public static ApiResponse<T> Fail(
        string message,
        string code,
        string? traceId = null,
        IReadOnlyDictionary<string, string[]>? errors = null) =>
        new(false, message, default, errors, code, traceId);
}
