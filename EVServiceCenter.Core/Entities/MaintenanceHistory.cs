using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

[Table("MaintenanceHistory")]
public partial class MaintenanceHistory
{
    [Key]
    [Column("HistoryID")]
    public int HistoryId { get; set; }

    [Column("VehicleID")]
    public int VehicleId { get; set; }

    [Column("WorkOrderID")]
    public int WorkOrderId { get; set; }

    public DateOnly ServiceDate { get; set; }

    public int? Mileage { get; set; }

    public string? ServicesPerformed { get; set; }

    public string? PartsReplaced { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalServiceCost { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalPartsCost { get; set; }

    [Column(TypeName = "decimal(16, 2)")]
    public decimal? TotalCost { get; set; }

    public DateOnly? NextServiceDue { get; set; }

    public int? NextServiceMileage { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? BatteryHealthBefore { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? BatteryHealthAfter { get; set; }

    [StringLength(500)]
    public string? DiagnosticCodes { get; set; }

    [StringLength(1000)]
    public string? TechnicianNotes { get; set; }

    [StringLength(1000)]
    public string? CustomerNotes { get; set; }

    public int? ServiceRating { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey("VehicleId")]
    [InverseProperty("MaintenanceHistories")]
    public virtual CustomerVehicle Vehicle { get; set; } = null!;

    [ForeignKey("WorkOrderId")]
    [InverseProperty("MaintenanceHistories")]
    public virtual WorkOrder WorkOrder { get; set; } = null!;
}
