using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class WorkOrderPart
{
    [Key]
    [Column("WorkOrderPartID")]
    public int WorkOrderPartId { get; set; }

    [Column("WorkOrderID")]
    public int WorkOrderId { get; set; }

    [Column("PartID")]
    public int PartId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? UnitCost { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? UnitPrice { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? DiscountAmount { get; set; }

    [Column(TypeName = "decimal(26, 2)")]
    public decimal? TotalCost { get; set; }

    [Column(TypeName = "decimal(27, 2)")]
    public decimal? TotalPrice { get; set; }

    public int? WarrantyPeriod { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public DateTime? RequestedDate { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public DateTime? InstalledDate { get; set; }

    public int? InstalledBy { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [ForeignKey("InstalledBy")]
    [InverseProperty("WorkOrderParts")]
    public virtual User? InstalledByNavigation { get; set; }

    [ForeignKey("PartId")]
    [InverseProperty("WorkOrderParts")]
    public virtual Part Part { get; set; } = null!;

    [ForeignKey("WorkOrderId")]
    [InverseProperty("WorkOrderParts")]
    public virtual WorkOrder WorkOrder { get; set; } = null!;
}
