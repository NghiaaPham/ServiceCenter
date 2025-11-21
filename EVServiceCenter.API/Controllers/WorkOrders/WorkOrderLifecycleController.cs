using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.WorkOrders;

/// <summary>
/// Work Order Lifecycle Management
/// Handles status changes, assignments, and work progression
/// </summary>
[ApiController]
[Route("api/work-orders")]
[ApiExplorerSettings(GroupName = "WorkOrder Lifecycle")]
[Authorize]
public class WorkOrderLifecycleController : BaseController
{
    private readonly IWorkOrderService _workOrderService;
    private readonly ILogger<WorkOrderLifecycleController> _logger;

    public WorkOrderLifecycleController(
        IWorkOrderService workOrderService,
        ILogger<WorkOrderLifecycleController> logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;
    }

    /// <summary>
    /// Admin-only endpoint to backfill existing WorkOrders with Appointment linkage and financials.
    /// Useful after schema changes to populate newly added fields from existing appointments.
    /// </summary>
    [HttpPost("admin/backfill-from-appointments")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> BackfillFromAppointments(CancellationToken cancellationToken)
    {
        try
        {
            var updated = await _workOrderService.BackfillWorkOrdersFromAppointmentsAsync(cancellationToken);
            return Success(updated, $"Backfill complete. Updated {updated} work orders.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during backfill from appointments");
            return ValidationError(ex.Message);
        }
    }

    #region Status Management

    /// <summary>
    /// Update work order status with notes
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="request">Status update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated work order with new status</returns>
    [HttpPatch("{id:int}/status")]
    [Authorize(Policy = "AdminOrStaff")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateWorkOrderStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workOrderService.UpdateStatusAsync(id, request, userId, cancellationToken);
            return Success(result, "Work order status updated successfully");
        }
        catch (KeyNotFoundException)
        {
            return NotFoundError($"Work order {id} not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work order status {WorkOrderId}", id);
            return ValidationError(ex.Message);
        }
    }

    #endregion

    #region Assignment & Workflow

    /// <summary>
    /// Assign technician to work order
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="technicianId">Technician ID to assign</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated work order with assigned technician</returns>
    [HttpPatch("{id:int}/assign-technician/{technicianId:int}")]
    [Authorize(Policy = "AdminOrStaff")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignTechnician(
        int id,
        int technicianId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workOrderService.AssignTechnicianAsync(id, technicianId, userId, cancellationToken);
            return Success(result, "Technician assigned successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning technician to work order {WorkOrderId}", id);
            return ValidationError(ex.Message);
        }
    }

    /// <summary>
    /// [Technician] Start work on work order
    /// 
    /// **Business Rules:**
    /// - Must be assigned to this work order
    /// - Must be on-shift (checked in)
    /// - Work order must be in Assigned or Created status
    /// 
    /// **On-Shift Validation:**
    /// System checks if technician has checked in for shift today.
    /// If not checked in, returns error with instruction to check-in first.
    /// </summary>
    [HttpPost("{id:int}/start")]
    [Authorize(Policy = "TechnicianOnly")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> StartWork(
        int id,
        [FromQuery] bool skipShiftValidation = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workOrderService.StartWorkAsync(id, userId, skipShiftValidation, cancellationToken);
            return Success(result, "Work started successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error starting work on {WorkOrderId} by user {UserId}", id, GetCurrentUserId());
            return ValidationError(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting work on work order {WorkOrderId}", id);
            return ServerError("An error occurred while starting work");
        }
    }

    /// <summary>
    /// [Technician] Complete work order
    /// 
    /// **Business Rules:**
    /// - Must be assigned to this work order
    /// - Work order must be InProgress
    /// - All required checklist items must be completed
    /// 
    /// **Post-Completion:**
    /// - Auto-generates invoice
    /// - Creates maintenance history
    /// - Updates appointment status
    /// </summary>
    [HttpPost("{id:int}/complete")]
    [Authorize(Policy = "TechnicianOnly")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteWorkOrder(int id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workOrderService.CompleteWorkOrderAsync(id, userId, cancellationToken);
            return Success(result, "Work order completed successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error completing {WorkOrderId} by user {UserId}", id, GetCurrentUserId());
            return ValidationError(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing work order {WorkOrderId}", id);
            return ServerError("An error occurred while completing work order");
        }
    }

    /// <summary>
    /// Admin/Staff helper: Force complete a work order for testing (bypass technician-only constraint).
    /// Use only for testing; will perform normal completion flow (invoice/payment intent generation).
    /// </summary>
    [HttpPost("{id:int}/force-complete")]
    [Authorize(Policy = "AdminOrStaff")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForceCompleteWorkOrder(int id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workOrderService.CompleteWorkOrderAsync(id, userId, cancellationToken);
            return Success(result, "Work order force-completed successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Validation error completing {WorkOrderId} by user {UserId}", id, GetCurrentUserId());
            return ValidationError(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing work order {WorkOrderId}", id);
            return ServerError("An error occurred while completing work order");
        }
    }

    #endregion

    #region Service & Part Management

    /// <summary>
    /// Add service to work order
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="serviceId">Service ID to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated work order with new service</returns>
    [HttpPost("{id:int}/services/{serviceId:int}")]
    [Authorize(Policy = "StaffOrTechnician")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddService(
        int id,
        int serviceId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workOrderService.AddServiceAsync(id, serviceId, userId, cancellationToken);
            return Success(result, "Service added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding service to work order {WorkOrderId}", id);
            return ValidationError(ex.Message);
        }
    }

    /// <summary>
    /// Add part to work order with quantity
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="partId">Part ID to add</param>
    /// <param name="quantity">Quantity of part</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated work order with new part</returns>
    [HttpPost("{id:int}/parts/{partId:int}")]
    [Authorize(Policy = "StaffOrTechnician")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddPart(
        int id,
        int partId,
        [FromQuery] int quantity,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workOrderService.AddPartAsync(id, partId, quantity, userId, cancellationToken);
            return Success(result, "Part added successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding part to work order {WorkOrderId}", id);
            return ValidationError(ex.Message);
        }
    }

    #endregion
}
