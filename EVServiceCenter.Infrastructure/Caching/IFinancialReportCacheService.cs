using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Requests;
using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;

namespace EVServiceCenter.Infrastructure.Caching;

/// <summary>
/// ✅ Enhancement #4: Redis distributed caching for Financial Reports
/// Senior Architect 20-year experience - Enterprise caching strategy
/// </summary>
public interface IFinancialReportCacheService
{
    /// <summary>
    /// Get or create revenue report with caching
    /// Cache key pattern: "revenue:{hash}" where hash = MD5(query params)
    /// TTL: 5 minutes for real-time data, 1 hour for historical data
    /// </summary>
    Task<RevenueReportResponseDto> GetOrCreateRevenueReportAsync(
        RevenueReportQueryDto query,
        Func<Task<RevenueReportResponseDto>> factory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or create payment report with caching
    /// Cache key pattern: "payment:{hash}"
    /// TTL: 5 minutes for today's data, 1 hour for historical data
    /// </summary>
    Task<PaymentReportResponseDto> GetOrCreatePaymentReportAsync(
        PaymentReportQueryDto query,
        Func<Task<PaymentReportResponseDto>> factory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or create invoice report with caching
    /// Cache key pattern: "invoice:{hash}"
    /// TTL: 10 minutes (aging analysis changes frequently)
    /// </summary>
    Task<InvoiceReportResponseDto> GetOrCreateInvoiceReportAsync(
        InvoiceReportQueryDto query,
        Func<Task<InvoiceReportResponseDto>> factory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or create profit report with caching
    /// Cache key pattern: "profit:{hash}"
    /// TTL: 15 minutes
    /// </summary>
    Task<ProfitReportResponseDto> GetOrCreateProfitReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? centerId,
        bool includeBreakdown,
        Func<Task<ProfitReportResponseDto>> factory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get or create popular services report with caching
    /// Cache key pattern: "services:{hash}"
    /// TTL: 30 minutes (service popularity changes slowly)
    /// </summary>
    Task<PopularServicesReportResponseDto> GetOrCreatePopularServicesReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? centerId,
        int topN,
        bool includeTrends,
        Func<Task<PopularServicesReportResponseDto>> factory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate all report caches
    /// Call this when data changes (e.g., payment completed, invoice updated)
    /// </summary>
    Task InvalidateAllAsync();

    /// <summary>
    /// Invalidate specific cache by key pattern
    /// </summary>
    Task InvalidateByPatternAsync(string pattern);
}
