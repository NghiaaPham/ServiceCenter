namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Revenue data point for time series analysis
/// Represents revenue for a specific time period (day, week, or month)
/// </summary>
public class RevenueTimeSeriesDto
{
    /// <summary>
    /// Period start date
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Period end date
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Period label (e.g., "2025-01-15", "Week 3", "January 2025")
    /// </summary>
    public string PeriodLabel { get; set; } = null!;

    /// <summary>
    /// Total revenue for this period
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Number of completed payments
    /// </summary>
    public int PaymentCount { get; set; }

    /// <summary>
    /// Number of invoices generated
    /// </summary>
    public int InvoiceCount { get; set; }

    /// <summary>
    /// Average payment amount
    /// </summary>
    public decimal AveragePaymentAmount { get; set; }

    /// <summary>
    /// Highest single payment amount
    /// </summary>
    public decimal MaxPaymentAmount { get; set; }

    /// <summary>
    /// Lowest single payment amount
    /// </summary>
    public decimal MinPaymentAmount { get; set; }
}
