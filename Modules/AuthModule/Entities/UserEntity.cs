using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetSkeleton.Modules.AuthModule.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Modules.AuthModule.Entities;

[Table("user")]
[Index(nameof(PublicId), IsUnique = true)]
[Index(nameof(Phone), IsUnique = true)]
public sealed class UserEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("public_id")]
    public Guid PublicId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("phone")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    // PasswordHasher stores a salted PBKDF2 hash here; never store the raw password.
    [Required]
    [Column("password")]
    [MaxLength(512)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Column("is_active")]
    public bool IsActive { get; set; }

    // Registration and phone verification are complete; IsActive remains an independent admin-controlled state.
    [Required]
    [Column("is_verified")]
    public bool IsVerified { get; set; }

    // Durable account verification time; AuthOtpEntity.VerifiedAt is scoped to one OTP challenge.
    [Column("verified_at")]
    public DateTimeOffset? VerifiedAt { get; set; }

    [Column("last_active_at")]
    public DateTimeOffset? LastActiveAt { get; set; }

    [Required]
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [Column("role")]
    public UserRole Role { get; set; } = UserRole.User;

    [Column("permission_id")]
    public int? PermissionId { get; set; }

    [ForeignKey(nameof(PermissionId))]
    public PermissionEntity? Permission { get; set; }

    public ICollection<AuthSessionEntity> AuthSessions { get; set; } = new List<AuthSessionEntity>();
    public ICollection<AuthOtpEntity> AuthOtps { get; set; } = new List<AuthOtpEntity>();
}
