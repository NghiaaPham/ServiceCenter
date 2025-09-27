using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class DataRetentionPolicy
{
    [Key]
    [Column("PolicyID")]
    public int PolicyId { get; set; }

    [StringLength(100)]
    public string TableName { get; set; } = null!;

    public int RetentionPeriodMonths { get; set; }

    [StringLength(100)]
    public string? ArchiveTableName { get; set; }

    public bool? DeleteAfterArchive { get; set; }

    [StringLength(500)]
    public string? RetentionCondition { get; set; }

    public DateTime? LastExecuted { get; set; }

    public DateTime? NextExecution { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("DataRetentionPolicies")]
    public virtual User? CreatedByNavigation { get; set; }
}
