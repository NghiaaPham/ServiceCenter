using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class PerformanceMetric
{
    [Key]
    [Column("MetricID")]
    public int MetricId { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    public DateOnly MetricDate { get; set; }

    public int? WorkOrdersCompleted { get; set; }

    public int? ServicesCompleted { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? AverageServiceTime { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? EfficiencyRating { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? CustomerRatingAvg { get; set; }

    public int? ReworkCount { get; set; }

    public int? ComplaintCount { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? HoursWorked { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? OvertimeHours { get; set; }

    public int? LateCount { get; set; }

    public int? AbsentCount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? RevenueGenerated { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? TrainingHoursCompleted { get; set; }

    public int? CertificationsEarned { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("PerformanceMetrics")]
    public virtual User User { get; set; } = null!;
}
