using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthUserCannotBeActivatedBeforeVerificationException()
    : AppException(
        StatusCodes.Status400BadRequest,
        "Không thể kích hoạt tài khoản trước khi xác minh số điện thoại.",
        "user_phone_not_verified");
