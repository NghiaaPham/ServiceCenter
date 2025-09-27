using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class VehicleHealthMetric
{
    [Key]
    [Column("MetricID")]
    public int MetricId { get; set; }

    [Column("VehicleID")]
    public int VehicleId { get; set; }

    [Column("WorkOrderID")]
    public int? WorkOrderId { get; set; }

    public DateOnly MetricDate { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? BatteryHealth { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? MotorEfficiency { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? BrakeWear { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? TireWear { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? OverallCondition { get; set; }

    [StringLength(500)]
    public string? DiagnosticCodes { get; set; }

    [StringLength(1000)]
    public string? Recommendations { get; set; }

    public DateOnly? NextCheckDue { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("VehicleHealthMetrics")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("VehicleId")]
    [InverseProperty("VehicleHealthMetrics")]
    public virtual CustomerVehicle Vehicle { get; set; } = null!;

    [ForeignKey("WorkOrderId")]
    [InverseProperty("VehicleHealthMetrics")]
    public virtual WorkOrder? WorkOrder { get; set; }
}
