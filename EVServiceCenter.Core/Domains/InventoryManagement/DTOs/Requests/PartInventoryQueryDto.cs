namespace EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;

/// <summary>
/// Query parameters for filtering and paginating part inventory
/// </summary>
public class PartInventoryQueryDto
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Search by part code, name, or barcode
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by service center
    /// </summary>
    public int? ServiceCenterId { get; set; }

    /// <summary>
    /// Filter by part category
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Filter by supplier
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Show only low stock items (CurrentStock <= ReorderLevel)
    /// </summary>
    public bool? IsLowStock { get; set; }

    /// <summary>
    /// Show only out of stock items (CurrentStock = 0)
    /// </summary>
    public bool? IsOutOfStock { get; set; }

    /// <summary>
    /// Show only active parts
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Sort by: partCode, partName, currentStock, lastUpdated (default: partCode)
    /// </summary>
    public string SortBy { get; set; } = "partCode";

    /// <summary>
    /// Sort direction: asc, desc (default: asc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
}
