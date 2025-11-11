namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Profit analysis report
/// Calculates net profit after all costs and fees
/// </summary>
public class ProfitReportResponseDto
{
    /// <summary>
    /// Report metadata
    /// </summary>
    public DateTime GeneratedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Revenue metrics
    /// </summary>
    public decimal TotalRevenue { get; set; }
    public decimal CollectedRevenue { get; set; }
    public decimal UncollectedRevenue { get; set; }

    /// <summary>
    /// Cost breakdown
    /// </summary>
    public decimal TotalCosts { get; set; }
    public decimal PaymentProcessingFees { get; set; }
    public decimal TotalDiscountsGiven { get; set; }
    public decimal RefundsIssued { get; set; }

    /// <summary>
    /// Profit calculation
    /// </summary>
    public decimal GrossProfit { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }

    /// <summary>
    /// Profitability ratios
    /// </summary>
    public decimal CollectionEfficiency { get; set; }
    public decimal CostToRevenueRatio { get; set; }
    public decimal AverageTransactionProfit { get; set; }

    /// <summary>
    /// Period comparison
    /// </summary>
    public decimal? ProfitGrowthRate { get; set; }
    public decimal? RevenueGrowthRate { get; set; }

    /// <summary>
    /// Service center breakdown (optional)
    /// </summary>
    public List<ServiceCenterProfitDto>? ServiceCenterBreakdown { get; set; }
}

/// <summary>
/// Profit breakdown by service center
/// </summary>
public class ServiceCenterProfitDto
{
    public int CenterId { get; set; }
    public string CenterName { get; set; } = null!;
    public decimal Revenue { get; set; }
    public decimal Costs { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
}
