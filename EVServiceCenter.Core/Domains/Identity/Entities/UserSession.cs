using EVServiceCenter.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Domains.Identity.Entities;

public partial class UserSession
{
    [Key]
    [Column("SessionID")]
    public int SessionId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(128)]
    public string SessionToken { get; set; } = null!;

    public DateTime? LoginTime { get; set; }

    public DateTime? LastActivityTime { get; set; }

    public DateTime? LogoutTime { get; set; }

    [Column("IPAddress")]
    [StringLength(45)]
    public string? Ipaddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("Session")]
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    [ForeignKey("UserId")]
    [InverseProperty("UserSessions")]
    public virtual User User { get; set; } = null!;
}
