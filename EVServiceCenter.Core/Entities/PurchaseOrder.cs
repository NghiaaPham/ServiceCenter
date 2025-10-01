using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class PurchaseOrder
{
    [Key]
    [Column("PurchaseOrderID")]
    public int PurchaseOrderId { get; set; }

    [Column("PONumber")]
    [StringLength(20)]
    public string Ponumber { get; set; } = null!;

    [Column("SupplierID")]
    public int SupplierId { get; set; }

    [Column("CenterID")]
    public int CenterId { get; set; }

    public DateOnly? OrderDate { get; set; }

    public DateOnly? RequiredDate { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? SubTotal { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TaxAmount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? ShippingCost { get; set; }

    [Column(TypeName = "decimal(17, 2)")]
    public decimal? TotalAmount { get; set; }

    public int? ApprovedBy { get; set; }

    public DateOnly? ApprovedDate { get; set; }

    public DateOnly? ReceivedDate { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey("ApprovedBy")]
    [InverseProperty("PurchaseOrderApprovedByNavigations")]
    public virtual User? ApprovedByNavigation { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("PurchaseOrders")]
    public virtual ServiceCenter Center { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    [InverseProperty("PurchaseOrderCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("PurchaseOrder")]
    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    [ForeignKey("SupplierId")]
    [InverseProperty("PurchaseOrders")]
    public virtual Supplier Supplier { get; set; } = null!;
}
