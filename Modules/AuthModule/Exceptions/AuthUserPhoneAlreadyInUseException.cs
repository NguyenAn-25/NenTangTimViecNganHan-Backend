using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthUserPhoneAlreadyInUseException()
    : AppException(
        StatusCodes.Status409Conflict,
        "Số điện thoại đã được sử dụng.",
        "phone_already_in_use");
