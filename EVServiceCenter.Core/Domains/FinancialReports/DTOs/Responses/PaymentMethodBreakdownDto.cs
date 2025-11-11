namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Revenue breakdown by payment method
/// </summary>
public class PaymentMethodBreakdownDto
{
    /// <summary>
    /// Payment method name (Cash, BankTransfer, VNPay, MoMo, Card)
    /// </summary>
    public string PaymentMethod { get; set; } = null!;

    /// <summary>
    /// Total number of transactions
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Total revenue amount in VND
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Percentage of total revenue
    /// </summary>
    public decimal Percentage { get; set; }

    /// <summary>
    /// Average transaction amount
    /// </summary>
    public decimal AverageAmount { get; set; }

    /// <summary>
    /// Processing fees (for gateway payments)
    /// </summary>
    public decimal? ProcessingFees { get; set; }

    /// <summary>
    /// Net revenue (TotalAmount - ProcessingFees)
    /// </summary>
    public decimal NetRevenue { get; set; }
}
