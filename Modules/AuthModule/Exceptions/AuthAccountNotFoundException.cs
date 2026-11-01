using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthAccountNotFoundException()
    : AppException(
        StatusCodes.Status404NotFound,
        "Không tìm thấy tài khoản.");
