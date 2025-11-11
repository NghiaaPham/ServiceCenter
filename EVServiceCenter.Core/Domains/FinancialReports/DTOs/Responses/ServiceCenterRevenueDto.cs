namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Revenue breakdown by service center
/// </summary>
public class ServiceCenterRevenueDto
{
    /// <summary>
    /// Service center ID
    /// </summary>
    public int CenterId { get; set; }

    /// <summary>
    /// Service center name
    /// </summary>
    public string CenterName { get; set; } = null!;

    /// <summary>
    /// Total number of invoices
    /// </summary>
    public int InvoiceCount { get; set; }

    /// <summary>
    /// Total revenue amount in VND
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Percentage of total company revenue
    /// </summary>
    public decimal Percentage { get; set; }

    /// <summary>
    /// Average invoice amount
    /// </summary>
    public decimal AverageInvoiceAmount { get; set; }

    /// <summary>
    /// Number of completed work orders
    /// </summary>
    public int CompletedWorkOrders { get; set; }

    /// <summary>
    /// Revenue from services only
    /// </summary>
    public decimal ServiceRevenue { get; set; }

    /// <summary>
    /// Revenue from parts only
    /// </summary>
    public decimal PartsRevenue { get; set; }
}
