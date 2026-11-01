using DotNetSkeleton.Shared.Exceptions;

namespace DotNetSkeleton.Modules.AuthModule.Exceptions;

public sealed class AuthOtpInvalidOrExpiredException()
    : AppException(
        StatusCodes.Status400BadRequest,
        "Mã OTP không hợp lệ hoặc đã hết hạn.",
        "invalid_or_expired_otp");
