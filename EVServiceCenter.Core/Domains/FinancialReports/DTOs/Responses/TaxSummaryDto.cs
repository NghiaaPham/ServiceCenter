namespace EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

/// <summary>
/// Tax collection summary report
/// </summary>
public class TaxSummaryDto
{
    /// <summary>
    /// Total tax collected in the period
    /// </summary>
    public decimal TotalTaxCollected { get; set; }

    /// <summary>
    /// Total taxable amount (subtotal before tax)
    /// </summary>
    public decimal TotalTaxableAmount { get; set; }

    /// <summary>
    /// Average tax rate applied
    /// </summary>
    public decimal AverageTaxRate { get; set; }

    /// <summary>
    /// Tax breakdown by rate
    /// </summary>
    public List<TaxRateBreakdownDto> TaxRateBreakdown { get; set; } = new();
}

/// <summary>
/// Tax collection breakdown by tax rate
/// </summary>
public class TaxRateBreakdownDto
{
    /// <summary>
    /// Tax rate percentage (e.g., 10 for 10% VAT)
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Number of invoices with this tax rate
    /// </summary>
    public int InvoiceCount { get; set; }

    /// <summary>
    /// Total taxable amount for this rate
    /// </summary>
    public decimal TaxableAmount { get; set; }

    /// <summary>
    /// Total tax collected at this rate
    /// </summary>
    public decimal TaxAmount { get; set; }
}
