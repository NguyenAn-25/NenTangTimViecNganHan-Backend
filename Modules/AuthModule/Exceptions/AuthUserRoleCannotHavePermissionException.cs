using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthUserRoleCannotHavePermissionException()
    : AppException(
        StatusCodes.Status400BadRequest,
        "Chỉ tài khoản ADMIN mới có permission profile.",
        "user_role_cannot_have_permission");
