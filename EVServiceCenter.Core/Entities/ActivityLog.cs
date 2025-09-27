using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class ActivityLog
{
    [Key]
    [Column("LogID")]
    public int LogId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("SessionID")]
    public int? SessionId { get; set; }

    [StringLength(50)]
    public string ActionType { get; set; } = null!;

    [StringLength(100)]
    public string? TableName { get; set; }

    [Column("RecordID")]
    public int? RecordId { get; set; }

    [StringLength(100)]
    public string? FieldName { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? ChangeDetails { get; set; }

    [Column("IPAddress")]
    [StringLength(45)]
    public string? Ipaddress { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    [StringLength(200)]
    public string? DeviceInfo { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [StringLength(10)]
    public string? Severity { get; set; }

    public bool? Success { get; set; }

    [StringLength(1000)]
    public string? ErrorMessage { get; set; }

    public int? ExecutionTime { get; set; }

    public DateTime? ChangeDate { get; set; }

    [ForeignKey("SessionId")]
    [InverseProperty("ActivityLogs")]
    public virtual UserSession? Session { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("ActivityLogs")]
    public virtual User User { get; set; } = null!;
}
