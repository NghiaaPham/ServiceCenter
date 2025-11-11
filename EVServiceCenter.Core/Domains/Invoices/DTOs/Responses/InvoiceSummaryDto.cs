namespace EVServiceCenter.Core.Domains.Invoices.DTOs.Responses;

/// <summary>
/// Summary DTO for invoice list (lighter than full response)
/// </summary>
public class InvoiceSummaryDto
{
    public int InvoiceId { get; set; }
    public string InvoiceCode { get; set; } = null!;
    public int? WorkOrderId { get; set; }
    public string? WorkOrderCode { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }

    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysOverdue { get; set; }

    public decimal GrandTotal { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal PaymentProgress { get; set; }

    public string Status { get; set; } = null!;
    public bool SentToCustomer { get; set; }

    public DateTime CreatedDate { get; set; }
}
