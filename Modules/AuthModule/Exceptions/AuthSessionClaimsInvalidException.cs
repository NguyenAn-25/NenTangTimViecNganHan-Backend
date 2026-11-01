using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthSessionClaimsInvalidException()
    : AppException(
        StatusCodes.Status401Unauthorized,
        "Token thiếu thông tin phiên đăng nhập hợp lệ.");
