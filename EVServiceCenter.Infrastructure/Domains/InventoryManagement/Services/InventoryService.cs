using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Responses;
using EVServiceCenter.Core.Domains.InventoryManagement.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.InventoryManagement.Services;

/// <summary>
/// Service for inventory management business logic
/// Implements business rules, caching, and coordinates repositories
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IPartInventoryRepository _inventoryRepository;
    private readonly IStockTransactionRepository _transactionRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<InventoryService> _logger;

    private const string CACHE_KEY_LOW_STOCK = "inventory:low-stock:{0}";
    private const string CACHE_KEY_STOCK_VALUE = "inventory:total-value:{0}";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public InventoryService(
        IPartInventoryRepository inventoryRepository,
        IStockTransactionRepository transactionRepository,
        IMemoryCache cache,
        ILogger<InventoryService> logger)
    {
        _inventoryRepository = inventoryRepository;
        _transactionRepository = transactionRepository;
        _cache = cache;
        _logger = logger;
    }

    #region Query Operations

    public async Task<PagedResult<PartInventoryResponseDto>> GetInventoryAsync(
        PartInventoryQueryDto query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting inventory list with filters");
        return await _inventoryRepository.GetInventoryAsync(query, cancellationToken);
    }

    public async Task<PartInventoryResponseDto?> GetInventoryByPartAndCenterAsync(
        int partId,
        int serviceCenterId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting inventory for Part {PartId} at Center {CenterId}", partId, serviceCenterId);
        return await _inventoryRepository.GetByPartAndCenterAsync(partId, serviceCenterId, cancellationToken);
    }

    public async Task<List<LowStockAlertResponseDto>> GetLowStockAlertsAsync(
        int? serviceCenterId = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = string.Format(CACHE_KEY_LOW_STOCK, serviceCenterId ?? 0);

        if (_cache.TryGetValue<List<LowStockAlertResponseDto>>(cacheKey, out var cachedAlerts) && cachedAlerts != null)
        {
            _logger.LogInformation("Returning cached low stock alerts");
            return cachedAlerts;
        }

        _logger.LogInformation("Fetching low stock alerts from database");
        var alerts = await _inventoryRepository.GetLowStockAlertsAsync(serviceCenterId, cancellationToken);

        _cache.Set(cacheKey, alerts, CacheExpiration);
        return alerts;
    }

    #endregion

    #region Stock Adjustment Operations

    public async Task<StockTransactionResponseDto> AdjustStockAsync(
        StockAdjustmentRequestDto request,
        int adjustedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adjusting stock: Type={Type}, Part={PartId}, Qty={Qty}",
            request.TransactionType, request.PartId, request.Quantity);

        // Business Rule Validation
        ValidateStockAdjustment(request);

        // For OUT transactions, verify sufficient stock
        if (request.TransactionType.Equals("OUT", StringComparison.OrdinalIgnoreCase) && request.Quantity > 0)
        {
            var hasSufficientStock = await _inventoryRepository.HasSufficientStockAsync(
                request.PartId,
                request.ServiceCenterId,
                request.Quantity,
                cancellationToken);

            if (!hasSufficientStock)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for Part {request.PartId}. Cannot issue {request.Quantity} units.");
            }

            // Convert to negative quantity for OUT
            request.Quantity = -request.Quantity;
        }

        // Create transaction (handles inventory update atomically)
        var transaction = await _transactionRepository.CreateTransactionAsync(
            request, adjustedBy, cancellationToken);

        // Invalidate cache
        InvalidateInventoryCache(request.ServiceCenterId);

        _logger.LogInformation("Stock adjustment completed. TransactionId={TransactionId}", transaction.TransactionId);
        return transaction;
    }

    public async Task<bool> ReserveStockAsync(
        int partId,
        int serviceCenterId,
        int quantity,
        int reservedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reserving {Quantity} units of Part {PartId}", quantity, partId);

        // Business Rule: Quantity must be positive
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        var result = await _inventoryRepository.ReserveStockAsync(
            partId, serviceCenterId, quantity, reservedBy, cancellationToken);

        if (result)
        {
            InvalidateInventoryCache(serviceCenterId);
        }

        return result;
    }

    public async Task<bool> ReleaseReservedStockAsync(
        int partId,
        int serviceCenterId,
        int quantity,
        int releasedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Releasing {Quantity} reserved units of Part {PartId}", quantity, partId);

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        var result = await _inventoryRepository.ReleaseReservedStockAsync(
            partId, serviceCenterId, quantity, releasedBy, cancellationToken);

        if (result)
        {
            InvalidateInventoryCache(serviceCenterId);
        }

        return result;
    }

    public async Task<StockTransactionResponseDto> ConsumeReservedStockAsync(
        int partId,
        int serviceCenterId,
        int quantity,
        int workOrderId,
        int consumedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Consuming {Quantity} reserved units of Part {PartId} for WO {WorkOrderId}",
            quantity, partId, workOrderId);

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        // Business Rule: First release the reservation, then create OUT transaction
        var released = await _inventoryRepository.ReleaseReservedStockAsync(
            partId, serviceCenterId, quantity, consumedBy, cancellationToken);

        if (!released)
        {
            throw new InvalidOperationException("Failed to release reserved stock");
        }

        // Create OUT transaction
        var request = new StockAdjustmentRequestDto
        {
            PartId = partId,
            ServiceCenterId = serviceCenterId,
            TransactionType = "OUT",
            Quantity = -quantity, // Negative for OUT
            ReferenceType = "WO",
            ReferenceId = workOrderId,
            Notes = $"Consumed for Work Order #{workOrderId}"
        };

        var transaction = await _transactionRepository.CreateTransactionAsync(
            request, consumedBy, cancellationToken);

        InvalidateInventoryCache(serviceCenterId);

        return transaction;
    }

    #endregion

    #region Transaction History

    public async Task<PagedResult<StockTransactionResponseDto>> GetTransactionHistoryAsync(
        StockTransactionQueryDto query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting transaction history");
        return await _transactionRepository.GetTransactionsAsync(query, cancellationToken);
    }

    public async Task<List<StockTransactionResponseDto>> GetRecentTransactionsByPartAsync(
        int partId,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting recent {Limit} transactions for Part {PartId}", limit, partId);
        return await _transactionRepository.GetRecentTransactionsByPartAsync(partId, limit, cancellationToken);
    }

    #endregion

    #region Reporting & Analytics

    public async Task<decimal> GetTotalStockValueAsync(
        int? serviceCenterId = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = string.Format(CACHE_KEY_STOCK_VALUE, serviceCenterId ?? 0);

        if (_cache.TryGetValue<decimal>(cacheKey, out var cachedValue))
        {
            _logger.LogInformation("Returning cached total stock value");
            return cachedValue;
        }

        _logger.LogInformation("Calculating total stock value");
        var value = await _inventoryRepository.GetTotalStockValueAsync(serviceCenterId, cancellationToken);

        _cache.Set(cacheKey, value, CacheExpiration);
        return value;
    }

    public async Task<object> GetStockMovementSummaryAsync(
        int? serviceCenterId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting stock movement summary");
        return await _transactionRepository.GetStockMovementSummaryAsync(
            serviceCenterId, dateFrom, dateTo, cancellationToken);
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Validate stock adjustment business rules
    /// </summary>
    private void ValidateStockAdjustment(StockAdjustmentRequestDto request)
    {
        // Validate transaction type
        var validTypes = new[] { "IN", "OUT", "ADJUST", "TRANSFER" };
        if (!validTypes.Contains(request.TransactionType, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Invalid transaction type: {request.TransactionType}. Must be one of: {string.Join(", ", validTypes)}",
                nameof(request.TransactionType));
        }

        // Validate quantity
        if (request.Quantity == 0)
        {
            throw new ArgumentException("Quantity cannot be zero", nameof(request.Quantity));
        }

        // Business Rule: IN transactions must have supplier and unit cost
        if (request.TransactionType.Equals("IN", StringComparison.OrdinalIgnoreCase))
        {
            if (!request.SupplierId.HasValue)
            {
                _logger.LogWarning("IN transaction without supplier for Part {PartId}", request.PartId);
            }

            if (!request.UnitCost.HasValue)
            {
                _logger.LogWarning("IN transaction without unit cost for Part {PartId}", request.PartId);
            }
        }
    }

    /// <summary>
    /// Invalidate inventory-related cache entries
    /// </summary>
    private void InvalidateInventoryCache(int serviceCenterId)
    {
        _cache.Remove(string.Format(CACHE_KEY_LOW_STOCK, serviceCenterId));
        _cache.Remove(string.Format(CACHE_KEY_LOW_STOCK, 0)); // Global cache
        _cache.Remove(string.Format(CACHE_KEY_STOCK_VALUE, serviceCenterId));
        _cache.Remove(string.Format(CACHE_KEY_STOCK_VALUE, 0)); // Global cache

        _logger.LogDebug("Invalidated inventory cache for Center {CenterId}", serviceCenterId);
    }

    #endregion
}
