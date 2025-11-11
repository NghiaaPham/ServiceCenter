using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Requests;
using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.FinancialReports.Interfaces;

/// <summary>
/// Financial reporting service interface
/// Provides comprehensive financial analytics and reporting capabilities
/// </summary>
public interface IFinancialReportService
{
    /// <summary>
    /// Generate revenue report with flexible filtering and grouping
    /// Supports daily, weekly, and monthly aggregation
    /// </summary>
    /// <param name="query">Report query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive revenue report with multiple breakdowns</returns>
    Task<RevenueReportResponseDto> GenerateRevenueReportAsync(
        RevenueReportQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate payment analytics report
    /// Provides payment status distribution, gateway performance, and failure analysis
    /// </summary>
    /// <param name="query">Payment report query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive payment analytics report</returns>
    Task<PaymentReportResponseDto> GeneratePaymentReportAsync(
        PaymentReportQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate invoice analytics report
    /// Provides invoice status distribution, aging analysis, discount effectiveness, and tax summary
    /// </summary>
    /// <param name="query">Invoice report query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive invoice analytics report</returns>
    Task<InvoiceReportResponseDto> GenerateInvoiceReportAsync(
        InvoiceReportQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate profit analysis report
    /// Calculates net profit after all costs, fees, and discounts
    /// </summary>
    /// <param name="startDate">Report start date</param>
    /// <param name="endDate">Report end date</param>
    /// <param name="centerId">Filter by service center (optional)</param>
    /// <param name="includeServiceCenterBreakdown">Include breakdown by center</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Profit analysis report</returns>
    Task<ProfitReportResponseDto> GenerateProfitReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? centerId = null,
        bool includeServiceCenterBreakdown = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate popular services report
    /// Shows most frequently used and highest revenue services
    /// </summary>
    /// <param name="startDate">Report start date</param>
    /// <param name="endDate">Report end date</param>
    /// <param name="centerId">Filter by service center (optional)</param>
    /// <param name="topN">Number of top services to return (default: 10)</param>
    /// <param name="includeTrends">Include period-over-period trend analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Popular services report</returns>
    Task<PopularServicesReportResponseDto> GeneratePopularServicesReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? centerId = null,
        int topN = 10,
        bool includeTrends = false,
        CancellationToken cancellationToken = default);
}
