namespace EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;

/// <summary>
/// Query parameters for stock transaction history
/// </summary>
public class StockTransactionQueryDto
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Filter by part ID
    /// </summary>
    public int? PartId { get; set; }

    /// <summary>
    /// Filter by service center
    /// </summary>
    public int? ServiceCenterId { get; set; }

    /// <summary>
    /// Filter by transaction type: IN, OUT, ADJUST, TRANSFER
    /// </summary>
    public string? TransactionType { get; set; }

    /// <summary>
    /// Filter by reference type: PO, WO, ADJ
    /// </summary>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// Filter by supplier
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Start date for date range filter
    /// </summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>
    /// End date for date range filter
    /// </summary>
    public DateTime? DateTo { get; set; }

    /// <summary>
    /// Sort by: transactionDate, quantity, totalCost (default: transactionDate desc)
    /// </summary>
    public string SortBy { get; set; } = "transactionDate";

    /// <summary>
    /// Sort direction: asc, desc (default: desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}
