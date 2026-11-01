using System.ComponentModel.DataAnnotations;

namespace DotNetSkeleton.Modules.AuthModule.DTOs.Requests;

public sealed class UpdateUserRequestDto
{
    [Required(ErrorMessage = "Quyền hạn là bắt buộc!")]
    [Range(1, int.MaxValue, ErrorMessage = "Quyền hạn phải là số nguyên dương bắt đầu từ 1!")]
    public int PermissionId { get; init; }
}
