namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Invoice distribution by status
/// </summary>
public class InvoiceStatusDistributionDto
{
    /// <summary>
    /// Invoice status (Pending, Paid, Cancelled, PartiallyPaid, Overdue)
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Number of invoices with this status
    /// </summary>
    public int InvoiceCount { get; set; }

    /// <summary>
    /// Total amount for invoices with this status
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Percentage of total invoices
    /// </summary>
    public decimal PercentageOfTotal { get; set; }

    /// <summary>
    /// Average invoice amount for this status
    /// </summary>
    public decimal AverageAmount { get; set; }
}
