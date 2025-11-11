namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Payment status distribution metrics
/// </summary>
public class PaymentStatusDistributionDto
{
    /// <summary>
    /// Payment status (Completed, Pending, Failed, etc.)
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// Number of payments with this status
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Percentage of total payments
    /// </summary>
    public decimal Percentage { get; set; }

    /// <summary>
    /// Total amount for payments with this status
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Average amount for payments with this status
    /// </summary>
    public decimal AverageAmount { get; set; }
}
