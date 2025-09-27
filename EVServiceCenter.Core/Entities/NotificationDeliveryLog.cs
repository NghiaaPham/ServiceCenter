using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

[Table("NotificationDeliveryLog")]
public partial class NotificationDeliveryLog
{
    [Key]
    [Column("LogID")]
    public int LogId { get; set; }

    [Column("NotificationID")]
    public int NotificationId { get; set; }

    public int AttemptNumber { get; set; }

    public DateTime? AttemptDate { get; set; }

    [StringLength(20)]
    public string? DeliveryStatus { get; set; }

    [StringLength(10)]
    public string? ResponseCode { get; set; }

    [StringLength(500)]
    public string? ResponseMessage { get; set; }

    public int? DeliveryTime { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public decimal? Cost { get; set; }

    [StringLength(50)]
    public string? ProviderName { get; set; }

    [ForeignKey("NotificationId")]
    [InverseProperty("NotificationDeliveryLogs")]
    public virtual Notification Notification { get; set; } = null!;
}
