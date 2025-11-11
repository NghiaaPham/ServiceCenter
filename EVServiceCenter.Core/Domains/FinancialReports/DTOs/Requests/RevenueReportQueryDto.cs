namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Requests;

/// <summary>
/// Query parameters for revenue report generation
/// Supports flexible filtering by date range, service center, and payment method
/// </summary>
public class RevenueReportQueryDto
{
    /// <summary>
    /// Start date for revenue analysis (required)
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date for revenue analysis (required)
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Filter by specific service center (optional)
    /// </summary>
    public int? CenterId { get; set; }

    /// <summary>
    /// Filter by payment method: Cash, BankTransfer, VNPay, MoMo (optional)
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Report grouping: Daily, Weekly, Monthly
    /// Default: Daily
    /// </summary>
    public string GroupBy { get; set; } = "Daily";

    /// <summary>
    /// Include detailed breakdown by payment method
    /// Default: true
    /// </summary>
    public bool IncludePaymentMethodBreakdown { get; set; } = true;

    /// <summary>
    /// Include detailed breakdown by service center
    /// Default: false (only if CenterId is not specified)
    /// </summary>
    public bool IncludeServiceCenterBreakdown { get; set; } = false;
}
