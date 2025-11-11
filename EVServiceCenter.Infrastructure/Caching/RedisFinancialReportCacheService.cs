using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Requests;
using EVServiceCenter.Core.Domains.FinancialReports.DTOs.Responses;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Caching;

/// <summary>
/// ✅ Enhancement #4: Redis distributed cache implementation
/// Senior Architect pattern: Cache-Aside with automatic TTL
/// </summary>
public class RedisFinancialReportCacheService : IFinancialReportCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisFinancialReportCacheService> _logger;

    // Cache TTL constants (Senior Architect best practice: centralized config)
    private static readonly TimeSpan TodayDataTTL = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan HistoricalDataTTL = TimeSpan.FromHours(1);
    private static readonly TimeSpan InvoiceTTL = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan ProfitTTL = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ServicesTTL = TimeSpan.FromMinutes(30);

    public RedisFinancialReportCacheService(
        IDistributedCache cache,
        ILogger<RedisFinancialReportCacheService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RevenueReportResponseDto> GetOrCreateRevenueReportAsync(
        RevenueReportQueryDto query,
        Func<Task<RevenueReportResponseDto>> factory,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey("revenue", query);
        var ttl = DetermineTTL(query.StartDate, query.EndDate);

        return await GetOrCreateAsync(cacheKey, factory, ttl, cancellationToken);
    }

    public async Task<PaymentReportResponseDto> GetOrCreatePaymentReportAsync(
        PaymentReportQueryDto query,
        Func<Task<PaymentReportResponseDto>> factory,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey("payment", query);
        var ttl = DetermineTTL(query.StartDate, query.EndDate);

        return await GetOrCreateAsync(cacheKey, factory, ttl, cancellationToken);
    }

    public async Task<InvoiceReportResponseDto> GetOrCreateInvoiceReportAsync(
        InvoiceReportQueryDto query,
        Func<Task<InvoiceReportResponseDto>> factory,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey("invoice", query);

        return await GetOrCreateAsync(cacheKey, factory, InvoiceTTL, cancellationToken);
    }

    public async Task<ProfitReportResponseDto> GetOrCreateProfitReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? centerId,
        bool includeBreakdown,
        Func<Task<ProfitReportResponseDto>> factory,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey("profit", new
        {
            startDate = startDate.ToString("yyyy-MM-dd"),
            endDate = endDate.ToString("yyyy-MM-dd"),
            centerId,
            includeBreakdown
        });

        return await GetOrCreateAsync(cacheKey, factory, ProfitTTL, cancellationToken);
    }

    public async Task<PopularServicesReportResponseDto> GetOrCreatePopularServicesReportAsync(
        DateTime startDate,
        DateTime endDate,
        int? centerId,
        int topN,
        bool includeTrends,
        Func<Task<PopularServicesReportResponseDto>> factory,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = GenerateCacheKey("services", new
        {
            startDate = startDate.ToString("yyyy-MM-dd"),
            endDate = endDate.ToString("yyyy-MM-dd"),
            centerId,
            topN,
            includeTrends
        });

        return await GetOrCreateAsync(cacheKey, factory, ServicesTTL, cancellationToken);
    }

    public async Task InvalidateAllAsync()
    {
        _logger.LogInformation("🗑️ Invalidating all financial report caches");

        // Note: Redis doesn't support pattern-based deletion easily
        // In production, use Redis SCAN command or tag-based invalidation
        // For now, we'll document the need for manual invalidation or time-based expiry

        // Alternative: Use cache tags or separate Redis database
        await Task.CompletedTask;

        _logger.LogWarning("⚠️ Full cache invalidation requires Redis SCAN - caches will expire naturally via TTL");
    }

    public async Task InvalidateByPatternAsync(string pattern)
    {
        _logger.LogInformation("🗑️ Invalidating caches matching pattern: {Pattern}", pattern);

        // Production implementation would use Redis SCAN + DEL
        // Or use a cache tag system
        await Task.CompletedTask;

        _logger.LogWarning("⚠️ Pattern-based invalidation not implemented - caches will expire via TTL");
    }

    #region Private Helper Methods

    /// <summary>
    /// Generic cache-aside pattern implementation
    /// </summary>
    private async Task<T> GetOrCreateAsync<T>(
        string cacheKey,
        Func<Task<T>> factory,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        try
        {
            // Try to get from cache first
            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);

            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("✅ Cache HIT: {CacheKey}", cacheKey);

                return JsonSerializer.Deserialize<T>(cachedData)!;
            }

            _logger.LogDebug("❌ Cache MISS: {CacheKey}", cacheKey);

            // Cache miss - generate data
            var data = await factory();

            // Store in cache
            var serializedData = JsonSerializer.Serialize(data, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl
            };

            await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions, cancellationToken);

            _logger.LogInformation("💾 Cached data: {CacheKey}, TTL: {TTL}s", cacheKey, ttl.TotalSeconds);

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Cache error for key: {CacheKey}. Falling back to direct query.", cacheKey);

            // Graceful degradation: return uncached data
            return await factory();
        }
    }

    /// <summary>
    /// Generate cache key using MD5 hash of query parameters
    /// Pattern: "fr:{prefix}:{hash}"
    /// </summary>
    private string GenerateCacheKey(string prefix, object query)
    {
        var queryJson = JsonSerializer.Serialize(query, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var hash = ComputeMD5Hash(queryJson);

        return $"fr:{prefix}:{hash}";
    }

    /// <summary>
    /// Compute MD5 hash for cache key generation
    /// </summary>
    private static string ComputeMD5Hash(string input)
    {
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    /// <summary>
    /// Determine TTL based on date range
    /// Senior Architect pattern: Dynamic TTL based on data characteristics
    /// </summary>
    private static TimeSpan DetermineTTL(DateTime startDate, DateTime endDate)
    {
        var today = DateTime.Today;

        // If query includes today's data, use shorter TTL
        if (endDate.Date >= today)
        {
            return TodayDataTTL;
        }

        // Historical data can be cached longer
        return HistoricalDataTTL;
    }

    #endregion
}
