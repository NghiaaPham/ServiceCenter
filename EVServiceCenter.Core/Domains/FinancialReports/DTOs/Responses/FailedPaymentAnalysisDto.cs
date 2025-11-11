namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Analysis of failed payments by error code
/// </summary>
public class FailedPaymentAnalysisDto
{
    /// <summary>
    /// Error/Response code from gateway
    /// </summary>
    public string ResponseCode { get; set; } = null!;

    /// <summary>
    /// Error message description
    /// </summary>
    public string ResponseMessage { get; set; } = null!;

    /// <summary>
    /// Number of failures with this error code
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Percentage of total failures
    /// </summary>
    public decimal PercentageOfFailures { get; set; }

    /// <summary>
    /// Total amount of failed transactions
    /// </summary>
    public decimal TotalFailedAmount { get; set; }

    /// <summary>
    /// Gateway name where failures occurred
    /// </summary>
    public string? GatewayName { get; set; }
}
