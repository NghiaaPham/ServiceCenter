using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class PurchaseOrderDetail
{
    [Key]
    [Column("PODetailID")]
    public int PodetailId { get; set; }

    [Column("PurchaseOrderID")]
    public int PurchaseOrderId { get; set; }

    [Column("PartID")]
    public int PartId { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? UnitCost { get; set; }

    [Column(TypeName = "decimal(26, 2)")]
    public decimal? TotalCost { get; set; }

    public int? ReceivedQuantity { get; set; }

    public int? RemainingQuantity { get; set; }

    [ForeignKey("PartId")]
    [InverseProperty("PurchaseOrderDetails")]
    public virtual Part Part { get; set; } = null!;

    [ForeignKey("PurchaseOrderId")]
    [InverseProperty("PurchaseOrderDetails")]
    public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
}
