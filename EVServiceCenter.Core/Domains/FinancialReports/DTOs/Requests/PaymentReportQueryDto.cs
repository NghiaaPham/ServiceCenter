namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Requests;

/// <summary>
/// Query parameters for payment analytics report
/// </summary>
public class PaymentReportQueryDto
{
    /// <summary>
    /// Start date for analysis (required)
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date for analysis (required)
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Filter by specific service center (optional)
    /// </summary>
    public int? CenterId { get; set; }

    /// <summary>
    /// Filter by payment method (optional)
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// Filter by payment status (optional)
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// Include gateway performance metrics
    /// Default: true
    /// </summary>
    public bool IncludeGatewayMetrics { get; set; } = true;

    /// <summary>
    /// Include failed payment analysis
    /// Default: true
    /// </summary>
    public bool IncludeFailureAnalysis { get; set; } = true;
}
