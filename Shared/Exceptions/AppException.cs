namespace DotNetSkeleton.Shared.Exceptions;

/// <summary>
/// Expected application error that can be translated to an HTTP response by the shared handler.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(
        int statusCode,
        string message,
        string? code = null,
        IReadOnlyDictionary<string, string[]>? errors = null)
        : base(message)
    {
        if (statusCode is < 400 or > 599)
            throw new ArgumentOutOfRangeException(nameof(statusCode), "Application errors must use an HTTP error status.");

        StatusCode = statusCode;
        Code = code ?? GetDefaultCode(statusCode);
        Errors = errors;
    }

    public int StatusCode { get; }
    public string Code { get; }
    public IReadOnlyDictionary<string, string[]>? Errors { get; }

    private static string GetDefaultCode(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "bad_request",
            StatusCodes.Status401Unauthorized => "unauthorized",
            StatusCodes.Status403Forbidden => "forbidden",
            StatusCodes.Status404NotFound => "not_found",
            StatusCodes.Status409Conflict => "conflict",
            StatusCodes.Status503ServiceUnavailable => "service_unavailable",
            _ => "application_error"
        };
}
