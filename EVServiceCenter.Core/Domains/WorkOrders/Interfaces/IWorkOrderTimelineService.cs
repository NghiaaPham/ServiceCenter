using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.WorkOrders.Interfaces;

/// <summary>
/// Service interface for WorkOrder timeline tracking
/// Manages timeline events and audit trail
/// </summary>
public interface IWorkOrderTimelineService
{
    /// <summary>
    /// Add timeline event to work order
    /// Auto-records who and when
    /// </summary>
    Task<WorkOrderTimelineResponseDto> AddTimelineEventAsync(
        int workOrderId,
        AddWorkOrderTimelineRequestDto request,
        int performedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all timeline events for work order
    /// Ordered by date descending (newest first)
    /// </summary>
    Task<List<WorkOrderTimelineResponseDto>> GetTimelineAsync(
        int workOrderId,
        bool includeHiddenEvents = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get customer-visible timeline events only
    /// </summary>
    Task<List<WorkOrderTimelineResponseDto>> GetCustomerTimelineAsync(
        int workOrderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Auto-create timeline event for status change
    /// Internal helper method
    /// </summary>
    Task AddStatusChangeEventAsync(
        int workOrderId,
        string fromStatus,
        string toStatus,
        string? notes,
        int performedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Auto-create timeline event for technician assignment
    /// </summary>
    Task AddTechnicianAssignmentEventAsync(
        int workOrderId,
        string technicianName,
        int performedBy,
        CancellationToken cancellationToken = default);
}
