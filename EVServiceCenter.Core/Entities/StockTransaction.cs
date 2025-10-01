using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class StockTransaction
{
    [Key]
    [Column("TransactionID")]
    public int TransactionId { get; set; }

    [Column("PartID")]
    public int PartId { get; set; }

    [Column("CenterID")]
    public int? CenterId { get; set; }

    [StringLength(50)]
    public string TransactionType { get; set; } = null!;

    public int Quantity { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? UnitCost { get; set; }

    [Column(TypeName = "decimal(26, 2)")]
    public decimal? TotalCost { get; set; }

    [StringLength(50)]
    public string? ReferenceType { get; set; }

    [Column("ReferenceID")]
    public int? ReferenceId { get; set; }

    public DateTime? TransactionDate { get; set; }

    [Column("SupplierID")]
    public int? SupplierId { get; set; }

    [StringLength(50)]
    public string? InvoiceNumber { get; set; }

    [StringLength(50)]
    public string? BatchNumber { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CenterId")]
    [InverseProperty("StockTransactions")]
    public virtual ServiceCenter? Center { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("StockTransactions")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("PartId")]
    [InverseProperty("StockTransactions")]
    public virtual Part Part { get; set; } = null!;

    [ForeignKey("SupplierId")]
    [InverseProperty("StockTransactions")]
    public virtual Supplier? Supplier { get; set; }
}
