using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthRefreshTokenInvalidException()
    : AppException(
        StatusCodes.Status401Unauthorized,
        "Refresh token không hợp lệ hoặc đã hết hạn.");
