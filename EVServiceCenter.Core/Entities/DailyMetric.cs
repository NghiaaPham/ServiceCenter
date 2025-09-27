using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class DailyMetric
{
    [Key]
    [Column("DailyMetricID")]
    public int DailyMetricId { get; set; }

    [Column("CenterID")]
    public int? CenterId { get; set; }

    public DateOnly MetricDate { get; set; }

    public int? AppointmentsScheduled { get; set; }

    public int? AppointmentsCompleted { get; set; }

    public int? AppointmentsCancelled { get; set; }

    public int? WorkOrdersCreated { get; set; }

    public int? WorkOrdersCompleted { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? DailyRevenue { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? ServiceRevenue { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? PartsRevenue { get; set; }

    public int? NewCustomers { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? CustomerSatisfactionAvg { get; set; }

    [Column(TypeName = "decimal(8, 2)")]
    public decimal? AverageServiceTime { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TechnicianUtilization { get; set; }

    public int? ReworkCount { get; set; }

    public int? WarrantyClaimsCount { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("DailyMetrics")]
    public virtual ServiceCenter? Center { get; set; }
}
