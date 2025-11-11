namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Invoice aging analysis by age bracket
/// Tracks outstanding invoices grouped by days overdue
/// </summary>
public class InvoiceAgingBracketDto
{
    /// <summary>
    /// Age bracket description (e.g., "0-30 days", "31-60 days")
    /// </summary>
    public string AgeBracket { get; set; } = null!;

    /// <summary>
    /// Minimum days in bracket
    /// </summary>
    public int MinDays { get; set; }

    /// <summary>
    /// Maximum days in bracket (null for "90+" bracket)
    /// </summary>
    public int? MaxDays { get; set; }

    /// <summary>
    /// Number of invoices in this age bracket
    /// </summary>
    public int InvoiceCount { get; set; }

    /// <summary>
    /// Total amount outstanding in this bracket
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Percentage of total outstanding amount
    /// </summary>
    public decimal PercentageOfTotal { get; set; }

    /// <summary>
    /// Average amount per invoice in this bracket
    /// </summary>
    public decimal AverageAmount { get; set; }
}
