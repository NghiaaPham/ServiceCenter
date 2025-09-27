using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class Report
{
    [Key]
    [Column("ReportID")]
    public int ReportId { get; set; }

    [StringLength(20)]
    public string ReportCode { get; set; } = null!;

    [StringLength(100)]
    public string ReportName { get; set; } = null!;

    [StringLength(50)]
    public string ReportType { get; set; } = null!;

    [StringLength(50)]
    public string? ReportCategory { get; set; }

    public string? Parameters { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [Column("CenterID")]
    public int? CenterId { get; set; }

    public int? TotalRecords { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalAmount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? ProfitAmount { get; set; }

    [StringLength(1000)]
    public string? KeyInsights { get; set; }

    [StringLength(500)]
    public string? FilePath { get; set; }

    [StringLength(10)]
    public string? FileFormat { get; set; }

    public int? FileSize { get; set; }

    public int? GeneratedBy { get; set; }

    public DateTime? GeneratedDate { get; set; }

    public DateTime? LastAccessedDate { get; set; }

    public int? AccessCount { get; set; }

    public bool? IsScheduled { get; set; }

    [StringLength(20)]
    public string? ScheduleFrequency { get; set; }

    public DateTime? NextRunDate { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("Reports")]
    public virtual ServiceCenter? Center { get; set; }

    [ForeignKey("GeneratedBy")]
    [InverseProperty("Reports")]
    public virtual User? GeneratedByNavigation { get; set; }
}
