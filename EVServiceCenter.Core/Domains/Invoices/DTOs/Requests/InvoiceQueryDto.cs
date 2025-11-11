namespace EVServiceCenter.Core.Domains.Invoices.DTOs.Requests;

/// <summary>
/// Query parameters for invoice filtering and pagination
/// </summary>
public class InvoiceQueryDto
{
    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default: 20, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Search by invoice code or customer name
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by customer ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Filter by work order ID
    /// </summary>
    public int? WorkOrderId { get; set; }

    /// <summary>
    /// Filter by status (Draft, Unpaid, PartiallyPaid, Paid, Cancelled, Refunded)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Filter by invoice date from
    /// </summary>
    public DateTime? InvoiceDateFrom { get; set; }

    /// <summary>
    /// Filter by invoice date to
    /// </summary>
    public DateTime? InvoiceDateTo { get; set; }

    /// <summary>
    /// Filter by due date from
    /// </summary>
    public DateTime? DueDateFrom { get; set; }

    /// <summary>
    /// Filter by due date to
    /// </summary>
    public DateTime? DueDateTo { get; set; }

    /// <summary>
    /// Filter by overdue invoices
    /// </summary>
    public bool? IsOverdue { get; set; }

    /// <summary>
    /// Sort by field (invoiceDate, dueDate, grandTotal, status)
    /// </summary>
    public string SortBy { get; set; } = "invoiceDate";

    /// <summary>
    /// Sort direction (asc, desc)
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}
