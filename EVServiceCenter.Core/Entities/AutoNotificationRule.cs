using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class AutoNotificationRule
{
    [Key]
    [Column("RuleID")]
    public int RuleId { get; set; }

    [StringLength(100)]
    public string RuleName { get; set; } = null!;

    [Column("TemplateID")]
    public int TemplateId { get; set; }

    [StringLength(50)]
    public string? TriggerTable { get; set; }

    [StringLength(50)]
    public string? TriggerEvent { get; set; }

    [StringLength(1000)]
    public string? TriggerCondition { get; set; }

    [StringLength(10)]
    public string? Priority { get; set; }

    public int? MaxRetries { get; set; }

    public int? RetryInterval { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("AutoNotificationRules")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("TemplateId")]
    [InverseProperty("AutoNotificationRules")]
    public virtual NotificationTemplate Template { get; set; } = null!;
}
