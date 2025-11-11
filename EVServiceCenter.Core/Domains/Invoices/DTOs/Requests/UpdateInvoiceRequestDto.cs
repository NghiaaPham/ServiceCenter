namespace EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;

/// <summary>
/// Request DTO for updating invoice details
/// </summary>
public class UpdateInvoiceRequestDto
{
    /// <summary>
    /// Optional: Update due date
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Optional: Update payment terms
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Optional: Update notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Optional: Update status
    /// </summary>
    public string? Status { get; set; }
}
