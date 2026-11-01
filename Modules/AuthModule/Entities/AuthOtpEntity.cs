using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetSkeleton.Modules.AuthModule.Enums;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Modules.AuthModule.Entities;

[Table("auth_otps")]
[Index(nameof(PublicId), IsUnique = true)]
[Index(nameof(UserId), nameof(Purpose), nameof(CreatedAt))]
public sealed class AuthOtpEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("public_id")]
    public Guid PublicId { get; set; } = Guid.NewGuid();

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; } = null!;

    // Contains a PasswordHasher hash of the one-time code.
    [Required]
    [Column("code")]
    [MaxLength(512)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [Column("purpose")]
    public OtpPurpose Purpose { get; set; } = OtpPurpose.Registration;

    [Required]
    [Column("failed_attempts")]
    public int FailedAttempts { get; set; }

    [Required]
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [Column("expired_at")]
    public DateTimeOffset ExpiredAt { get; set; }

    [Column("verified_at")]
    public DateTimeOffset? VerifiedAt { get; set; }
}
