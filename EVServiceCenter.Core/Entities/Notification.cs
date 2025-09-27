using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class Notification
{
    [Key]
    [Column("NotificationID")]
    public int NotificationId { get; set; }

    [StringLength(20)]
    public string NotificationCode { get; set; } = null!;

    [Column("TemplateID")]
    public int? TemplateId { get; set; }

    [StringLength(20)]
    public string RecipientType { get; set; } = null!;

    [Column("UserID")]
    public int? UserId { get; set; }

    [Column("CustomerID")]
    public int? CustomerId { get; set; }

    [StringLength(20)]
    public string Channel { get; set; } = null!;

    [StringLength(10)]
    public string? Priority { get; set; }

    [StringLength(200)]
    public string? Subject { get; set; }

    public string Message { get; set; } = null!;

    [StringLength(200)]
    public string? RecipientAddress { get; set; }

    [StringLength(100)]
    public string? RecipientName { get; set; }

    public DateTime? ScheduledDate { get; set; }

    public DateTime? SendDate { get; set; }

    public DateTime? DeliveredDate { get; set; }

    public DateTime? ReadDate { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [StringLength(500)]
    public string? FailureReason { get; set; }

    public int? RetryCount { get; set; }

    [StringLength(50)]
    public string? RelatedType { get; set; }

    [Column("RelatedID")]
    public int? RelatedId { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("NotificationCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Notifications")]
    public virtual Customer? Customer { get; set; }

    [InverseProperty("Notification")]
    public virtual ICollection<NotificationDeliveryLog> NotificationDeliveryLogs { get; set; } = new List<NotificationDeliveryLog>();

    [ForeignKey("TemplateId")]
    [InverseProperty("Notifications")]
    public virtual NotificationTemplate? Template { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("NotificationUsers")]
    public virtual User? User { get; set; }
}
