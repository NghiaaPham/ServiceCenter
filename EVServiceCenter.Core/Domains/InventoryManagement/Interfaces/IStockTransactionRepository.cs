using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.InventoryManagement.Interfaces;

/// <summary>
/// Repository for stock transaction data access
/// </summary>
public interface IStockTransactionRepository
{
    /// <summary>
    /// Get paginated transaction history with filtering
    /// Uses optimized query with proper indexing
    /// </summary>
    Task<PagedResult<StockTransactionResponseDto>> GetTransactionsAsync(
        StockTransactionQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get transaction details by ID
    /// </summary>
    Task<StockTransactionResponseDto?> GetByIdAsync(
        int transactionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recent transactions for a specific part
    /// Performance: Indexed on PartId + TransactionDate desc
    /// </summary>
    Task<List<StockTransactionResponseDto>> GetRecentTransactionsByPartAsync(
        int partId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create stock transaction and update inventory
    /// Uses database transaction for atomicity
    /// </summary>
    Task<StockTransactionResponseDto> CreateTransactionAsync(
        StockAdjustmentRequestDto request,
        int createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get stock movement summary for reporting
    /// Aggregates IN/OUT/ADJUST transactions by date range
    /// </summary>
    Task<object> GetStockMovementSummaryAsync(
        int? serviceCenterId,
        DateTime? dateFrom,
        DateTime? dateTo,
        CancellationToken cancellationToken = default);
}
