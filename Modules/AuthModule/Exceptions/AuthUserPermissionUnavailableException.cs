using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthUserPermissionUnavailableException()
    : AppException(
        StatusCodes.Status400BadRequest,
        "Permission không tồn tại hoặc đang bị vô hiệu hóa.");
