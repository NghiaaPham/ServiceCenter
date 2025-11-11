using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Requests;
using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;
using EVServiceCenter.Core.Domains.FinancialReports.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Caching;

/// <summary>
/// ✅ Enhancement #4: Decorator pattern for transparent caching
/// Senior Architect pattern: Decorator wraps actual service with caching layer
///
/// Usage in DI:
/// services.AddScoped<IFinancialReportService, FinancialReportService>();
/// services.Decorate<IFinancialReportService, CachedFinancialReportService>();
/// </summary>
public class CachedFinancialReportService : IFinancialReportService
{
    private readonly IFinancialReportService _innerService;
    private readonly IFinancialReportCacheService _cacheService;
    private readonly ILogger<CachedFinancialReportService> _logger;

    public CachedFinancialReportService(
        IFinancialReportService innerService,
        IFinancialReportCacheService cacheService,
        ILogger<CachedFinancialReportService> logger)
    {
        _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RevenueReportResponseDto> GenerateRevenueReportAsync(
        RevenueReportQueryDto query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("📊 GenerateRevenueReportAsync with caching");

        return await _cacheService.GetOrCreateRevenueReportAsync(
            query,
            () => _innerService.GenerateRevenueReportAsync(query, cancellationToken),
            cancellationToken);
    }

    public async Task<PaymentReportResponseDto> GeneratePaymentReportAsync(
        PaymentReportQueryDto query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("💳 GeneratePaymentReportAsync with caching");

        return await _cacheService.GetOrCreatePaymentReportAsync(
            query,
            () => _innerService.GeneratePaymentReportAsync(query, cancellationToken),
            cancellationToken);
    }

    public async Task<InvoiceReportResponseDto> GenerateInvoiceReportAsync(
        InvoiceReportQueryDto query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("📄 GenerateInvoiceReportAsync with caching");

        return await _cacheService.GetOrCreateInvoiceReportAsync(
            query,
            () => _innerService.GenerateInvoiceReportAsync(query, cancellationToken),
            cancellationToken);
    }

    public async Task<ProfitReportResponseDto> GenerateProfitReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? centerId = null,
        bool includeServiceCenterBreakdown = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("💰 GenerateProfitReportAsync with caching");

        return await _cacheService.GetOrCreateProfitReportAsync(
            startDate,
            endDate,
            centerId,
            includeServiceCenterBreakdown,
            () => _innerService.GenerateProfitReportAsync(startDate, endDate, centerId, includeServiceCenterBreakdown, cancellationToken),
            cancellationToken);
    }

    public async Task<PopularServicesReportResponseDto> GeneratePopularServicesReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? centerId = null,
        int topN = 10,
        bool includeTrends = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("📈 GeneratePopularServicesReportAsync with caching");

        return await _cacheService.GetOrCreatePopularServicesReportAsync(
            startDate,
            endDate,
            centerId,
            topN,
            includeTrends,
            () => _innerService.GeneratePopularServicesReportAsync(startDate, endDate, centerId, topN, includeTrends, cancellationToken),
            cancellationToken);
    }
}
