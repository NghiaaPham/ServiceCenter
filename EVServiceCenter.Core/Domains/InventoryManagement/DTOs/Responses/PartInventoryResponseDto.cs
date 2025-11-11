namespace EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Responses;

/// <summary>
/// Part inventory information with stock levels
/// </summary>
public class PartInventoryResponseDto
{
    public int InventoryId { get; set; }
    public int PartId { get; set; }
    public string PartCode { get; set; } = null!;
    public string? BarCode { get; set; }
    public string PartName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public string? BrandName { get; set; }
    public string? Unit { get; set; }

    // Stock Information
    public int ServiceCenterId { get; set; }
    public string ServiceCenterName { get; set; } = null!;
    public int CurrentStock { get; set; }
    public int ReservedStock { get; set; }
    public int AvailableStock { get; set; }
    public int ReorderLevel { get; set; }
    public int? MinStock { get; set; }
    public int? MaxStock { get; set; }
    public string? Location { get; set; }

    // Stock Status (calculated)
    public string StockStatus { get; set; } = null!; // OK, LOW, OUT_OF_STOCK, OVERSTOCKED
    public bool NeedsReorder { get; set; }
    public int? ReorderQuantity { get; set; }

    // Pricing
    public decimal? CostPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal? TotalStockValue { get; set; } // CurrentStock * CostPrice

    // Additional Info
    public string? PartCondition { get; set; }
    public bool IsConsumable { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public string? SupplierName { get; set; }

    // Audit
    public DateOnly? LastStockTakeDate { get; set; }
    public DateTime? LastStockUpdateDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedByName { get; set; }
}
