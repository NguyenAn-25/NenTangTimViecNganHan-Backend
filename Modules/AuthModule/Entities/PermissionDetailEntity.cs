using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Modules.AuthModule.Entities;

[Table("permission_details")]
[Index(nameof(PermissionId), nameof(FunctionId), nameof(Action), IsUnique = true)]
public sealed class PermissionDetailEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("permission_id")]
    public int PermissionId { get; set; }

    [ForeignKey(nameof(PermissionId))]
    public PermissionEntity Permission { get; set; } = null!;

    [Required]
    [Column("function_id")]
    public int FunctionId { get; set; }

    [ForeignKey(nameof(FunctionId))]
    public FunctionEntity Function { get; set; } = null!;

    [Required]
    [Column("action")]
    [MaxLength(64)]
    public string Action { get; set; } = string.Empty;
}
