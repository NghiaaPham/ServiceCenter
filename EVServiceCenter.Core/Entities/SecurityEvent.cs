using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class SecurityEvent
{
    [Key]
    [Column("EventID")]
    public int EventId { get; set; }

    [StringLength(50)]
    public string EventType { get; set; } = null!;

    [Column("UserID")]
    public int? UserId { get; set; }

    [Column("IPAddress")]
    [StringLength(45)]
    public string? Ipaddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    [StringLength(1000)]
    public string? EventDescription { get; set; }

    [StringLength(20)]
    public string? RiskLevel { get; set; }

    public bool? IsBlocked { get; set; }

    [StringLength(500)]
    public string? BlockReason { get; set; }

    [StringLength(500)]
    public string? Resolution { get; set; }

    public int? ResolvedBy { get; set; }

    public DateTime? ResolvedDate { get; set; }

    public DateTime? EventDate { get; set; }

    [ForeignKey("ResolvedBy")]
    [InverseProperty("SecurityEventResolvedByNavigations")]
    public virtual User? ResolvedByNavigation { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("SecurityEventUsers")]
    public virtual User? User { get; set; }
}
