using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Modules.AuthModule.Entities;

[Table("functions")]
[Index(nameof(NameEn), IsUnique = true)]
public sealed class FunctionEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("name_vn")]
    [MaxLength(100)]
    public string NameVn { get; set; } = string.Empty;

    [Required]
    [Column("name_en")]
    [MaxLength(100)]
    public string NameEn { get; set; } = string.Empty;

    [Required]
    [Column("actions", TypeName = "text[]")]
    public string[] Actions { get; set; } = [];

    public ICollection<PermissionDetailEntity> PermissionDetails { get; set; } = new List<PermissionDetailEntity>();
}
