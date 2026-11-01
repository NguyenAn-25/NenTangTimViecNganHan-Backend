using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetSkeleton.Modules.AuthModule.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Modules.AuthModule.Entities;

[Table("permissions")]
[Index(nameof(Name), IsUnique = true)]
public sealed class PermissionEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("status")]
    public PermissionStatus Status { get; set; } = PermissionStatus.Active;

    [Required]
    [Column("name")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    [MaxLength(500)]
    public string? Description { get; set; }

    public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
    public ICollection<PermissionDetailEntity> Details { get; set; } = new List<PermissionDetailEntity>();
}
