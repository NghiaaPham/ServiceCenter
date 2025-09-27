using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class Supplier
{
    [Key]
    [Column("SupplierID")]
    public int SupplierId { get; set; }

    [StringLength(20)]
    public string SupplierCode { get; set; } = null!;

    [StringLength(100)]
    public string SupplierName { get; set; } = null!;

    [StringLength(100)]
    public string? ContactName { get; set; }

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? PaymentTerms { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? CreditLimit { get; set; }

    public int? Rating { get; set; }

    public bool? IsPreferred { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    [InverseProperty("Supplier")]
    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();

    [InverseProperty("Supplier")]
    public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

    [InverseProperty("Supplier")]
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();
}
