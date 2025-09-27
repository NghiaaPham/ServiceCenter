using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class WorkOrderService
{
    [Key]
    [Column("WorkOrderServiceID")]
    public int WorkOrderServiceId { get; set; }

    [Column("WorkOrderID")]
    public int WorkOrderId { get; set; }

    [Column("ServiceID")]
    public int ServiceId { get; set; }

    public int? Quantity { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? UnitPrice { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? DiscountAmount { get; set; }

    [Column(TypeName = "decimal(27, 2)")]
    public decimal? TotalPrice { get; set; }

    public int? EstimatedTime { get; set; }

    public int? ActualTime { get; set; }

    [Column("TechnicianID")]
    public int? TechnicianId { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? ProgressPercentage { get; set; }

    public DateTime? StartedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public int? QualityRating { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(500)]
    public string? InternalNotes { get; set; }

    [ForeignKey("ServiceId")]
    [InverseProperty("WorkOrderServices")]
    public virtual MaintenanceService Service { get; set; } = null!;

    [ForeignKey("TechnicianId")]
    [InverseProperty("WorkOrderServices")]
    public virtual User? Technician { get; set; }

    [ForeignKey("WorkOrderId")]
    [InverseProperty("WorkOrderServices")]
    public virtual WorkOrder WorkOrder { get; set; } = null!;
}
