using EVServiceCenter.Core.Domains.Shared.Interfaces;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Core.Domains.WorkOrders.Interfaces;

/// <summary>
/// Repository interface for WorkOrder data access
/// Provides optimized queries for work order operations
/// </summary>
public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    /// <summary>
    /// Get work orders with filtering, sorting, and pagination
    /// Optimized with includes for related data
    /// </summary>
    Task<PagedResult<WorkOrderSummaryDto>> GetWorkOrdersAsync(
        WorkOrderQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed work order by ID with all related data
    /// Includes services, parts, timeline, checklist
    /// </summary>
    Task<WorkOrderResponseDto?> GetWorkOrderDetailAsync(
        int workOrderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get work order by code
    /// </summary>
    Task<WorkOrder?> GetByCodeAsync(
        string workOrderCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate unique work order code
    /// Format: WO-YYYYMMDD-####
    /// </summary>
    Task<string> GenerateWorkOrderCodeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get work orders assigned to specific technician
    /// </summary>
    Task<List<WorkOrderSummaryDto>> GetWorkOrdersByTechnicianAsync(
        int technicianId,
        int? statusId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get work orders for specific vehicle
    /// Used for vehicle maintenance history
    /// </summary>
    Task<List<WorkOrderSummaryDto>> GetWorkOrdersByVehicleAsync(
        int vehicleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get work orders by customer
    /// </summary>
    Task<List<WorkOrderSummaryDto>> GetWorkOrdersByCustomerAsync(
        int customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if technician is available (has capacity for more work orders)
    /// </summary>
    Task<bool> IsTechnicianAvailableAsync(
        int technicianId,
        int maxConcurrentWorkOrders = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update work order status
    /// </summary>
    Task<bool> UpdateStatusAsync(
        int workOrderId,
        int newStatusId,
        int updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate progress percentage based on checklist completion
    /// </summary>
    Task<decimal> CalculateProgressAsync(
        int workOrderId,
        CancellationToken cancellationToken = default);
}
