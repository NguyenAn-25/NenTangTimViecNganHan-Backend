using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DotNetSkeleton.Modules.AuthModule.Entities;

[Table("auth_sessions")]
[Index(nameof(RefreshToken), IsUnique = true)]
[Index(nameof(UserId))]
public sealed class AuthSessionEntity
{
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("user_id")]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; } = null!;

    // SHA-256 hash of the opaque refresh token, not the token itself.
    [Required]
    [Column("refresh_token")]
    [MaxLength(64)]
    public string RefreshToken { get; set; } = string.Empty;

    [Required]
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    [Column("expired_at")]
    public DateTimeOffset ExpiredAt { get; set; }

    [Column("revoked_at")]
    public DateTimeOffset? RevokedAt { get; set; }
}
