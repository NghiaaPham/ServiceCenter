namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Comprehensive revenue report response
/// Contains aggregated revenue data with multiple breakdown dimensions
/// </summary>
public class RevenueReportResponseDto
{
    /// <summary>
    /// Report generation metadata
    /// </summary>
    public DateTime GeneratedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string GroupBy { get; set; } = null!;
    public int? CenterId { get; set; }
    public string? CenterName { get; set; }

    /// <summary>
    /// Summary metrics
    /// </summary>
    public decimal TotalRevenue { get; set; }
    public int TotalPaymentCount { get; set; }
    public int TotalInvoiceCount { get; set; }
    public decimal AveragePaymentAmount { get; set; }
    public decimal AverageInvoiceAmount { get; set; }

    /// <summary>
    /// Revenue breakdown by payment status
    /// </summary>
    public decimal CompletedPaymentsAmount { get; set; }
    public int CompletedPaymentsCount { get; set; }
    public decimal PendingPaymentsAmount { get; set; }
    public int PendingPaymentsCount { get; set; }
    public decimal FailedPaymentsAmount { get; set; }
    public int FailedPaymentsCount { get; set; }

    /// <summary>
    /// Collection rate metrics
    /// </summary>
    public decimal CollectionRate { get; set; } // CompletedAmount / TotalInvoiceAmount * 100
    public decimal OutstandingAmount { get; set; }

    /// <summary>
    /// Time series data (daily, weekly, or monthly breakdown)
    /// </summary>
    public List<RevenueTimeSeriesDto> TimeSeries { get; set; } = new();

    /// <summary>
    /// Revenue breakdown by payment method (optional)
    /// </summary>
    public List<PaymentMethodBreakdownDto>? PaymentMethodBreakdown { get; set; }

    /// <summary>
    /// Revenue breakdown by service center (optional)
    /// </summary>
    public List<ServiceCenterRevenueDto>? ServiceCenterBreakdown { get; set; }

    /// <summary>
    /// Growth comparison with previous period
    /// </summary>
    public decimal? GrowthRate { get; set; }
    public decimal? PreviousPeriodRevenue { get; set; }
}
