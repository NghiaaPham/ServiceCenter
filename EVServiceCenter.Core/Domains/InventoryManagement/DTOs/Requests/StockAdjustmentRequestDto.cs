namespace EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;

/// <summary>
/// Request to adjust stock levels (IN/OUT/ADJUST)
/// </summary>
public class StockAdjustmentRequestDto
{
    /// <summary>
    /// Part ID to adjust
    /// </summary>
    public int PartId { get; set; }

    /// <summary>
    /// Service center ID
    /// </summary>
    public int ServiceCenterId { get; set; }

    /// <summary>
    /// Transaction type: IN (receive), OUT (issue), ADJUST (correction), TRANSFER
    /// </summary>
    public string TransactionType { get; set; } = null!;

    /// <summary>
    /// Quantity to adjust (positive for IN, negative for OUT)
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit cost per item (for IN transactions)
    /// </summary>
    public decimal? UnitCost { get; set; }

    /// <summary>
    /// Reference type: PO (Purchase Order), WO (Work Order), ADJ (Adjustment)
    /// </summary>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// Reference ID (PO ID, WO ID, etc.)
    /// </summary>
    public int? ReferenceId { get; set; }

    /// <summary>
    /// Supplier ID (for IN transactions)
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Invoice number from supplier
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Batch/Lot number for tracking
    /// </summary>
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Expiry date (for consumables)
    /// </summary>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>
    /// Storage location in warehouse
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Notes/reason for adjustment
    /// </summary>
    public string? Notes { get; set; }
}
