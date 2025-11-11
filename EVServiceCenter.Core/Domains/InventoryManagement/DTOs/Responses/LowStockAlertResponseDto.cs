namespace EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Responses;

/// <summary>
/// Low stock alert information for parts that need reordering
/// </summary>
public class LowStockAlertResponseDto
{
    public int PartId { get; set; }
    public string PartCode { get; set; } = null!;
    public string PartName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;

    public int ServiceCenterId { get; set; }
    public string ServiceCenterName { get; set; } = null!;

    // Stock Levels
    public int CurrentStock { get; set; }
    public int ReservedStock { get; set; }
    public int AvailableStock { get; set; }
    public int ReorderLevel { get; set; }
    public int? MinStock { get; set; }

    // Alert Status
    public string AlertLevel { get; set; } = null!; // CRITICAL (out of stock), HIGH (< 25% of reorder), MEDIUM (< 50%), LOW (< 100%)
    public int StockShortage { get; set; } // ReorderLevel - CurrentStock
    public int SuggestedOrderQuantity { get; set; }

    // Supplier Info
    public int? PreferredSupplierId { get; set; }
    public string? PreferredSupplierName { get; set; }
    public decimal? LastPurchasePrice { get; set; }
    public DateTime? LastPurchaseDate { get; set; }

    // Usage Statistics (helpful for reorder decisions)
    public decimal? AverageMonthlyUsage { get; set; }
    public int? DaysUntilStockout { get; set; } // Estimated days until out of stock

    // Additional Info
    public string? Location { get; set; }
    public DateTime? LastStockUpdateDate { get; set; }
}
