namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Gateway performance metrics (VNPay, MoMo)
/// </summary>
public class GatewayPerformanceDto
{
    /// <summary>
    /// Gateway name (VNPay, MoMo)
    /// </summary>
    public string GatewayName { get; set; } = null!;

    /// <summary>
    /// Total number of payment attempts
    /// </summary>
    public int TotalAttempts { get; set; }

    /// <summary>
    /// Number of successful payments
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed payments
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// Number of pending payments
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// Success rate percentage
    /// </summary>
    public decimal SuccessRate { get; set; }

    /// <summary>
    /// Failure rate percentage
    /// </summary>
    public decimal FailureRate { get; set; }

    /// <summary>
    /// Total amount processed
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Total processing fees charged
    /// </summary>
    public decimal TotalProcessingFees { get; set; }

    /// <summary>
    /// Average transaction amount
    /// </summary>
    public decimal AverageTransactionAmount { get; set; }
}
