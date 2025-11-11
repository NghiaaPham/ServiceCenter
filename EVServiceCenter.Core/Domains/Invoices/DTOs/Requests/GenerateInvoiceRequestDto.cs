namespace EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;

/// <summary>
/// Request DTO for generating invoice from work order
/// </summary>
public class GenerateInvoiceRequestDto
{
    /// <summary>
    /// Work order ID to generate invoice from
    /// </summary>
    public int WorkOrderId { get; set; }

    /// <summary>
    /// Optional: Due date (default: 30 days from invoice date)
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Optional: Payment terms (e.g., "Net 30", "Due on Receipt")
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Optional: Additional notes for invoice
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Optional: Apply additional manual discount
    /// </summary>
    public decimal? AdditionalDiscountPercent { get; set; }

    /// <summary>
    /// Optional: Apply additional manual discount (fixed amount)
    /// </summary>
    public decimal? AdditionalDiscountAmount { get; set; }

    /// <summary>
    /// Optional: Send invoice to customer immediately
    /// </summary>
    public bool SendToCustomer { get; set; } = false;

    /// <summary>
    /// Optional: Send method (Email, SMS, Both)
    /// </summary>
    public string? SendMethod { get; set; }
}
