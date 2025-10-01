using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class Part
{
    [Key]
    [Column("PartID")]
    public int PartId { get; set; }

    [StringLength(50)]
    public string PartCode { get; set; } = null!;

    [StringLength(50)]
    public string? BarCode { get; set; }

    [StringLength(200)]
    public string PartName { get; set; } = null!;

    [Column("CategoryID")]
    public int CategoryId { get; set; }

    [Column("BrandID")]
    public int? BrandId { get; set; }

    [StringLength(20)]
    public string? Unit { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? CostPrice { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? SellingPrice { get; set; }

    public int? MinStock { get; set; }

    public int? CurrentStock { get; set; }

    public int? ReorderLevel { get; set; }

    public int? MaxStock { get; set; }

    [StringLength(100)]
    public string? Location { get; set; }

    [Column(TypeName = "decimal(10, 3)")]
    public decimal? Weight { get; set; }

    [StringLength(100)]
    public string? Dimensions { get; set; }

    public int? WarrantyPeriod { get; set; }

    [StringLength(20)]
    public string? PartCondition { get; set; }

    public bool? IsConsumable { get; set; }

    public bool? IsActive { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public string? TechnicalSpecs { get; set; }

    public string? CompatibleModels { get; set; }

    [Column("SupplierID")]
    public int? SupplierId { get; set; }

    [Column("AlternativePartIDs")]
    [StringLength(200)]
    public string? AlternativePartIds { get; set; }

    public DateTime? LastStockUpdateDate { get; set; }

    public DateTime? LastCostUpdateDate { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("BrandId")]
    [InverseProperty("Parts")]
    public virtual CarBrand? Brand { get; set; }

    [ForeignKey("CategoryId")]
    [InverseProperty("Parts")]
    public virtual PartCategory Category { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    [InverseProperty("Parts")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Part")]
    public virtual ICollection<PartInventory> PartInventories { get; set; } = new List<PartInventory>();

    [InverseProperty("Part")]
    public virtual ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();

    [InverseProperty("Part")]
    public virtual ICollection<StockTransaction> StockTransactions { get; set; } = new List<StockTransaction>();

    [ForeignKey("SupplierId")]
    [InverseProperty("Parts")]
    public virtual Supplier? Supplier { get; set; }

    [InverseProperty("Part")]
    public virtual ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();

    [InverseProperty("Part")]
    public virtual ICollection<WorkOrderPart> WorkOrderParts { get; set; } = new List<WorkOrderPart>();
}
