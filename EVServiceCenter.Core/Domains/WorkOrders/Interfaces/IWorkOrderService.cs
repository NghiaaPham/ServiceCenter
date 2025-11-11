using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.WorkOrders.Interfaces;

/// <summary>
/// Service interface for WorkOrder business logic
/// Handles work order lifecycle and operations
/// </summary>
public interface IWorkOrderService
{
    /// <summary>
    /// Create new work order from request
    /// Generates work order code, initializes timeline, creates checklist
    /// </summary>
    Task<WorkOrderResponseDto> CreateWorkOrderAsync(
        CreateWorkOrderRequestDto request,
        int createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create work order from appointment
    /// Auto-populates data from appointment
    /// </summary>
    Task<WorkOrderResponseDto> CreateWorkOrderFromAppointmentAsync(
        int appointmentId,
        int createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get work order details by ID
    /// </summary>
    Task<WorkOrderResponseDto> GetWorkOrderAsync(
        int workOrderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get work order by code
    /// </summary>
    Task<WorkOrderResponseDto> GetWorkOrderByCodeAsync(
        string workOrderCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get filtered and paginated work orders
    /// </summary>
    Task<PagedResult<WorkOrderSummaryDto>> GetWorkOrdersAsync(
        WorkOrderQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update work order details
    /// </summary>
    Task<WorkOrderResponseDto> UpdateWorkOrderAsync(
        int workOrderId,
        UpdateWorkOrderRequestDto request,
        int updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update work order status with timeline event
    /// Validates status transitions
    /// </summary>
    Task<WorkOrderResponseDto> UpdateStatusAsync(
        int workOrderId,
        UpdateWorkOrderStatusRequestDto request,
        int updatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assign technician to work order
    /// Checks technician availability and skills
    /// </summary>
    Task<WorkOrderResponseDto> AssignTechnicianAsync(
        int workOrderId,
        int technicianId,
        int assignedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Start work on work order
    /// Changes status to InProgress
    /// </summary>
    Task<WorkOrderResponseDto> StartWorkAsync(
        int workOrderId,
        int startedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete work order
    /// Validates all checklist items completed, creates invoice
    /// </summary>
    Task<WorkOrderResponseDto> CompleteWorkOrderAsync(
        int workOrderId,
        int completedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Request approval for work order
    /// Used when additional services/parts needed
    /// </summary>
    Task<WorkOrderResponseDto> RequestApprovalAsync(
        int workOrderId,
        string approvalNotes,
        int requestedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve work order
    /// </summary>
    Task<WorkOrderResponseDto> ApproveWorkOrderAsync(
        int workOrderId,
        string? approvalNotes,
        int approvedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add service to work order
    /// Updates estimated amount
    /// </summary>
    Task<WorkOrderResponseDto> AddServiceAsync(
        int workOrderId,
        int serviceId,
        int addedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add part to work order
    /// Updates estimated amount, checks inventory
    /// </summary>
    Task<WorkOrderResponseDto> AddPartAsync(
        int workOrderId,
        int partId,
        int quantity,
        int addedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete work order (soft delete)
    /// Only allowed if not started
    /// </summary>
    Task<bool> DeleteWorkOrderAsync(
        int workOrderId,
        int deletedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get work order statistics for dashboard
    /// </summary>
    Task<WorkOrderStatisticsDto> GetWorkOrderStatisticsAsync(
        int? serviceCenterId = null,
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Work order statistics DTO
/// </summary>
public class WorkOrderStatisticsDto
{
    public int TotalWorkOrders { get; set; }
    public int PendingWorkOrders { get; set; }
    public int InProgressWorkOrders { get; set; }
    public int CompletedWorkOrders { get; set; }
    public int CancelledWorkOrders { get; set; }
    public decimal AverageCompletionTimeHours { get; set; }
    public decimal TotalRevenue { get; set; }
}
