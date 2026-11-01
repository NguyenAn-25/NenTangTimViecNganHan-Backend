using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthUserNotFoundException()
    : AppException(
        StatusCodes.Status404NotFound,
        "Không tìm thấy người dùng.");
