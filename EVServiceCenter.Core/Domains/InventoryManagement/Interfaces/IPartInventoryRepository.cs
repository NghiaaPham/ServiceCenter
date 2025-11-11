using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.InventoryManagement.Interfaces;

/// <summary>
/// Repository for part inventory data access with performance optimization
/// </summary>
public interface IPartInventoryRepository
{
    /// <summary>
    /// Get paginated list of part inventory with advanced filtering
    /// Uses optimized query with includes for related entities
    /// </summary>
    Task<PagedResult<PartInventoryResponseDto>> GetInventoryAsync(
        PartInventoryQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get inventory for a specific part at a service center
    /// </summary>
    Task<PartInventoryResponseDto?> GetByPartAndCenterAsync(
        int partId,
        int serviceCenterId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get low stock alerts with intelligent sorting by criticality
    /// </summary>
    Task<List<LowStockAlertResponseDto>> GetLowStockAlertsAsync(
        int? serviceCenterId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total stock value across all service centers or specific center
    /// Performance: Single aggregate query
    /// </summary>
    Task<decimal> GetTotalStockValueAsync(
        int? serviceCenterId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if part has sufficient available stock
    /// Performance: Indexed query on PartId + CenterId
    /// </summary>
    Task<bool> HasSufficientStockAsync(
        int partId,
        int serviceCenterId,
        int requiredQuantity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reserve stock for work order (decrements AvailableStock, increments ReservedStock)
    /// Uses transaction for atomicity
    /// </summary>
    Task<bool> ReserveStockAsync(
        int partId,
        int serviceCenterId,
        int quantity,
        int reservedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Release reserved stock (reverts reservation)
    /// </summary>
    Task<bool> ReleaseReservedStockAsync(
        int partId,
        int serviceCenterId,
        int quantity,
        int releasedBy,
        CancellationToken cancellationToken = default);
}
