using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthUserClaimsInvalidException()
    : AppException(
        StatusCodes.Status401Unauthorized,
        "Token thiếu thông tin người dùng hợp lệ.");
