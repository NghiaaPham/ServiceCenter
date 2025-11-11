namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Comprehensive invoice analytics report
/// Provides detailed insights into invoice status, aging, discounts, and tax
/// </summary>
public class InvoiceReportResponseDto
{
    /// <summary>
    /// Report metadata
    /// </summary>
    public DateTime GeneratedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }

    /// <summary>
    /// Summary metrics
    /// </summary>
    public int TotalInvoices { get; set; }
    public decimal TotalInvoiceAmount { get; set; }
    public decimal AverageInvoiceAmount { get; set; }

    /// <summary>
    /// Outstanding invoices metrics
    /// </summary>
    public int OutstandingInvoicesCount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal OutstandingPercentage { get; set; }

    /// <summary>
    /// Paid invoices metrics
    /// </summary>
    public int PaidInvoicesCount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal CollectionRate { get; set; }

    /// <summary>
    /// Invoice status distribution
    /// </summary>
    public List<InvoiceStatusDistributionDto> StatusDistribution { get; set; } = new();

    /// <summary>
    /// Aging analysis (optional)
    /// Breakdown of outstanding invoices by age
    /// </summary>
    public List<InvoiceAgingBracketDto>? AgingAnalysis { get; set; }

    /// <summary>
    /// Discount effectiveness analysis (optional)
    /// </summary>
    public DiscountAnalysisDto? DiscountAnalysis { get; set; }

    /// <summary>
    /// Tax collection summary (optional)
    /// </summary>
    public TaxSummaryDto? TaxSummary { get; set; }

    /// <summary>
    /// Average days to payment (for paid invoices)
    /// </summary>
    public decimal? AverageDaysToPayment { get; set; }

    /// <summary>
    /// Most common invoice status
    /// </summary>
    public string MostCommonStatus { get; set; } = null!;
}
