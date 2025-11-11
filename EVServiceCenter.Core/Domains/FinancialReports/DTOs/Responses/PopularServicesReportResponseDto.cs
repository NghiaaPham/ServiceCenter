namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Popular services analysis report
/// Shows most frequently used and highest revenue services
/// </summary>
public class PopularServicesReportResponseDto
{
    /// <summary>
    /// Report metadata
    /// </summary>
    public DateTime GeneratedAt { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Summary metrics
    /// </summary>
    public int TotalServicesProvided { get; set; }
    public int UniqueServicesCount { get; set; }
    public decimal TotalServiceRevenue { get; set; }
    public decimal AverageServicePrice { get; set; }

    /// <summary>
    /// Most popular services by usage count
    /// </summary>
    public List<ServicePopularityDto> MostUsedServices { get; set; } = new();

    /// <summary>
    /// Highest revenue generating services
    /// </summary>
    public List<ServiceRevenueDto> HighestRevenueServices { get; set; } = new();

    /// <summary>
    /// Service category breakdown
    /// </summary>
    public List<ServiceCategoryStatsDto> CategoryBreakdown { get; set; } = new();

    /// <summary>
    /// Service trends (growing vs declining)
    /// </summary>
    public List<ServiceTrendDto>? ServiceTrends { get; set; }
}

/// <summary>
/// Service popularity by usage count
/// </summary>
public class ServicePopularityDto
{
    public int ServiceId { get; set; }
    public string ServiceCode { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public int UsageCount { get; set; }
    public decimal PercentageOfTotal { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AveragePrice { get; set; }
}

/// <summary>
/// Service revenue ranking
/// </summary>
public class ServiceRevenueDto
{
    public int ServiceId { get; set; }
    public string ServiceCode { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string CategoryName { get; set; } = null!;
    public decimal TotalRevenue { get; set; }
    public decimal PercentageOfTotal { get; set; }
    public int UsageCount { get; set; }
    public decimal AveragePrice { get; set; }
}

/// <summary>
/// Service category statistics
/// </summary>
public class ServiceCategoryStatsDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public int ServiceCount { get; set; }
    public int UsageCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PercentageOfTotal { get; set; }
    public decimal AverageServicePrice { get; set; }
}

/// <summary>
/// Service usage trend analysis
/// </summary>
public class ServiceTrendDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public int CurrentPeriodCount { get; set; }
    public int PreviousPeriodCount { get; set; }
    public decimal GrowthRate { get; set; }
    public string Trend { get; set; } = null!; // "Growing", "Stable", "Declining"
}
