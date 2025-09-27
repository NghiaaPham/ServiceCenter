using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class BusinessRule
{
    [Key]
    [Column("RuleID")]
    public int RuleId { get; set; }

    [StringLength(100)]
    public string RuleName { get; set; } = null!;

    [StringLength(50)]
    public string? Category { get; set; }

    [StringLength(50)]
    public string? RuleType { get; set; }

    public string? Condition { get; set; }

    public string? Action { get; set; }

    public int? Priority { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("BusinessRules")]
    public virtual User? CreatedByNavigation { get; set; }
}
