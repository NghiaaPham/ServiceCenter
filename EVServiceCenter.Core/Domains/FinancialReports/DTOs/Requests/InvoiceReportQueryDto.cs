namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Requests;

/// <summary>
/// Query parameters for invoice analytics report
/// Supports filtering by date range, status, service center, and aging analysis
/// </summary>
public class InvoiceReportQueryDto
{
    /// <summary>
    /// Report start date
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Report end date
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Filter by specific service center (optional)
    /// </summary>
    public int? CenterId { get; set; }

    /// <summary>
    /// Filter by invoice status (optional)
    /// Values: Pending, Paid, Cancelled, PartiallyPaid, Overdue
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Include aging analysis breakdown (0-30, 31-60, 61-90, 90+ days)
    /// Default: true
    /// </summary>
    public bool IncludeAgingAnalysis { get; set; } = true;

    /// <summary>
    /// Include discount effectiveness analysis
    /// Default: true
    /// </summary>
    public bool IncludeDiscountAnalysis { get; set; } = true;

    /// <summary>
    /// Include tax collection summary
    /// Default: false
    /// </summary>
    public bool IncludeTaxSummary { get; set; } = false;
}
