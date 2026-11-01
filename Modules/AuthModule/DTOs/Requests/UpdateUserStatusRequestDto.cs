using System.ComponentModel.DataAnnotations;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Requests;

public sealed class UpdateUserStatusRequestDto
{
    [Required(ErrorMessage = "Trạng thái tài khoản là bắt buộc!")]
    public bool? IsActive { get; init; }
}
