namespace EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Responses;

/// <summary>
/// Stock transaction details
/// </summary>
public class StockTransactionResponseDto
{
    public int TransactionId { get; set; }
    public int PartId { get; set; }
    public string PartCode { get; set; } = null!;
    public string PartName { get; set; } = null!;

    public int? ServiceCenterId { get; set; }
    public string? ServiceCenterName { get; set; }

    // Transaction Details
    public string TransactionType { get; set; } = null!; // IN, OUT, ADJUST, TRANSFER
    public int Quantity { get; set; }
    public int StockBefore { get; set; }
    public int StockAfter { get; set; }

    // Financial
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }

    // References
    public string? ReferenceType { get; set; } // PO, WO, ADJ
    public int? ReferenceId { get; set; }
    public string? ReferenceNumber { get; set; }

    // Supplier Info
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? InvoiceNumber { get; set; }

    // Tracking
    public string? BatchNumber { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? Location { get; set; }
    public string? Notes { get; set; }

    // Audit
    public DateTime TransactionDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
}
