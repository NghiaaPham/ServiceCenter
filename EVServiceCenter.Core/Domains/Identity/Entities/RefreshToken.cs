using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Domains.Identity.Entities;

[Table("RefreshTokens")]
public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(256)]
    public string Selector { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string TokenHash { get; set; } = null!;

    [Required]
    public DateTime Expires { get; set; }

    public DateTime Created { get; set; }

    [StringLength(100)]
    public string? CreatedByIp { get; set; }

    public DateTime? Revoked { get; set; }

    [StringLength(100)]
    public string? RevokedByIp { get; set; }

    [StringLength(500)]
    public string? ReplacedByTokenHash { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsRevoked => Revoked != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}