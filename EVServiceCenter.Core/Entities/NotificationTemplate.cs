using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class NotificationTemplate
{
    [Key]
    [Column("TemplateID")]
    public int TemplateId { get; set; }

    [StringLength(50)]
    public string TemplateCode { get; set; } = null!;

    [StringLength(100)]
    public string TemplateName { get; set; } = null!;

    [Column("TypeID")]
    public int TypeId { get; set; }

    [StringLength(20)]
    public string Channel { get; set; } = null!;

    [StringLength(200)]
    public string? Subject { get; set; }

    public string? MessageTemplate { get; set; }

    [StringLength(100)]
    public string? TriggerEvent { get; set; }

    [StringLength(500)]
    public string? TriggerCondition { get; set; }

    public int? SendDelay { get; set; }

    public bool? IsAutomatic { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [InverseProperty("Template")]
    public virtual ICollection<AutoNotificationRule> AutoNotificationRules { get; set; } = new List<AutoNotificationRule>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("NotificationTemplates")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Template")]
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    [ForeignKey("TypeId")]
    [InverseProperty("NotificationTemplates")]
    public virtual NotificationType Type { get; set; } = null!;
}
