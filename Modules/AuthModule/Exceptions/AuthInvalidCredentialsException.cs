using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthInvalidCredentialsException()
    : AppException(
        StatusCodes.Status401Unauthorized,
        "Thông tin đăng nhập không hợp lệ.");
