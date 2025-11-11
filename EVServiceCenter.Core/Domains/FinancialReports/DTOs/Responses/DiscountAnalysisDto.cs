namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Discount effectiveness analysis
/// Analyzes discount usage patterns and financial impact
/// </summary>
public class DiscountAnalysisDto
{
    /// <summary>
    /// Total number of invoices with discounts applied
    /// </summary>
    public int TotalInvoicesWithDiscount { get; set; }

    /// <summary>
    /// Total discount amount given
    /// </summary>
    public decimal TotalDiscountAmount { get; set; }

    /// <summary>
    /// Average discount percentage across all discounted invoices
    /// </summary>
    public decimal AverageDiscountPercentage { get; set; }

    /// <summary>
    /// Highest discount percentage applied
    /// </summary>
    public decimal MaxDiscountPercentage { get; set; }

    /// <summary>
    /// Discount amount as percentage of total revenue
    /// </summary>
    public decimal DiscountImpactOnRevenue { get; set; }

    /// <summary>
    /// Breakdown by discount percentage range
    /// </summary>
    public List<DiscountRangeBreakdownDto> DiscountRangeBreakdown { get; set; } = new();
}

/// <summary>
/// Discount usage breakdown by percentage range
/// </summary>
public class DiscountRangeBreakdownDto
{
    /// <summary>
    /// Discount range description (e.g., "0-10%", "11-20%")
    /// </summary>
    public string DiscountRange { get; set; } = null!;

    /// <summary>
    /// Number of invoices in this range
    /// </summary>
    public int InvoiceCount { get; set; }

    /// <summary>
    /// Total discount amount in this range
    /// </summary>
    public decimal TotalDiscountAmount { get; set; }

    /// <summary>
    /// Percentage of all discounted invoices
    /// </summary>
    public decimal PercentageOfDiscountedInvoices { get; set; }
}
