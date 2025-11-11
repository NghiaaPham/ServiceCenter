namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Comprehensive payment analytics report
/// </summary>
public class PaymentReportResponseDto
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
    public int TotalPayments { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AveragePaymentAmount { get; set; }
    public decimal TotalProcessingFees { get; set; }
    public decimal NetRevenue { get; set; }

    /// <summary>
    /// Overall success/failure metrics
    /// </summary>
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int PendingPayments { get; set; }
    public decimal OverallSuccessRate { get; set; }
    public decimal OverallFailureRate { get; set; }

    /// <summary>
    /// Payment status distribution
    /// </summary>
    public List<PaymentStatusDistributionDto> StatusDistribution { get; set; } = new();

    /// <summary>
    /// Payment method breakdown
    /// </summary>
    public List<PaymentMethodBreakdownDto> MethodBreakdown { get; set; } = new();

    /// <summary>
    /// Gateway performance comparison (optional)
    /// </summary>
    public List<GatewayPerformanceDto>? GatewayPerformance { get; set; }

    /// <summary>
    /// Failed payment analysis (optional)
    /// </summary>
    public List<FailedPaymentAnalysisDto>? FailureAnalysis { get; set; }

    /// <summary>
    /// Top payment methods by transaction count
    /// </summary>
    public string MostUsedPaymentMethod { get; set; } = null!;

    /// <summary>
    /// Most reliable gateway (highest success rate)
    /// </summary>
    public string? MostReliableGateway { get; set; }
}
