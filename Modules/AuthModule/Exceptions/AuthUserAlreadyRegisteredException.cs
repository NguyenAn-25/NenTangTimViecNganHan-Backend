using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthUserAlreadyRegisteredException()
    : AppException(
        StatusCodes.Status409Conflict,
        "Tài khoản với số điện thoại này đã được xác minh. Vui lòng đăng nhập hoặc dùng chức năng quên mật khẩu.",
        "auth_user_already_registered");
