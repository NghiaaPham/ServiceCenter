using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.InventoryManagement.Interfaces;

/// <summary>
/// Service for inventory management business logic
/// Coordinates between repositories and enforces business rules
/// </summary>
public interface IInventoryService
{
    // ========== Inventory Query Operations ==========

    /// <summary>
    /// Get paginated inventory list with caching for performance
    /// </summary>
    Task<PagedResult<PartInventoryResponseDto>> GetInventoryAsync(
        PartInventoryQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory for specific part and center
    /// </summary>
    Task<PartInventoryResponseDto?> GetInventoryByPartAndCenterAsync(
        int partId,
        int serviceCenterId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get low stock alerts with priority sorting
    /// Business Rule: Critical = out of stock, High = < 25%, Medium = < 50%, Low = < 100% of reorder level
    /// </summary>
    Task<List<LowStockAlertResponseDto>> GetLowStockAlertsAsync(
        int? serviceCenterId = null,
        CancellationToken cancellationToken = default);

    // ========== Stock Adjustment Operations ==========

    /// <summary>
    /// Adjust stock levels (IN/OUT/ADJUST/TRANSFER)
    /// Business Rules:
    /// - Validates sufficient stock for OUT transactions
    /// - Automatically updates AvailableStock
    /// - Creates audit trail in StockTransaction
    /// - Updates LastStockUpdateDate
    /// </summary>
    Task<StockTransactionResponseDto> AdjustStockAsync(
        StockAdjustmentRequestDto request,
        int adjustedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserve stock for work order
    /// Business Rules:
    /// - Validates AvailableStock >= quantity
    /// - Atomic operation: CurrentStock unchanged, AvailableStock--, ReservedStock++
    /// </summary>
    Task<bool> ReserveStockAsync(
        int partId,
        int serviceCenterId,
        int quantity,
        int reservedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Release reserved stock (cancel work order or return unused parts)
    /// </summary>
    Task<bool> ReleaseReservedStockAsync(
        int partId,
        int serviceCenterId,
        int quantity,
        int releasedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consume reserved stock (complete work order)
    /// Business Rule: CurrentStock--, ReservedStock--, creates OUT transaction
    /// </summary>
    Task<StockTransactionResponseDto> ConsumeReservedStockAsync(
        int partId,
        int serviceCenterId,
        int quantity,
        int workOrderId,
        int consumedBy,
        CancellationToken cancellationToken = default);

    // ========== Transaction History ==========

    /// <summary>
    /// Get transaction history with filtering
    /// </summary>
    Task<PagedResult<StockTransactionResponseDto>> GetTransactionHistoryAsync(
        StockTransactionQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent transactions for a part
    /// </summary>
    Task<List<StockTransactionResponseDto>> GetRecentTransactionsByPartAsync(
        int partId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    // ========== Reporting & Analytics ==========

    /// <summary>
    /// Get total stock value for financial reporting
    /// </summary>
    Task<decimal> GetTotalStockValueAsync(
        int? serviceCenterId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get stock movement summary for period
    /// Returns aggregated IN/OUT/ADJUST statistics
    /// </summary>
    Task<object> GetStockMovementSummaryAsync(
        int? serviceCenterId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken cancellationToken = default);
}
