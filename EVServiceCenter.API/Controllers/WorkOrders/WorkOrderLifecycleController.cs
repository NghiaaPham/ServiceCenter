using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.Interfaces;
using EVServiceCenter.Core.Domains.Invoices.DTOs.Responses;
using EVServiceCenter.Core.Domains.Invoices.Interfaces;
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
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<WorkOrderLifecycleController> _logger;

    public WorkOrderLifecycleController(
        IWorkOrderService workOrderService,
        IInvoiceService invoiceService,
        ILogger<WorkOrderLifecycleController> logger)
    {
        _workOrderService = workOrderService;
        _invoiceService = invoiceService;
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
    /// [Technician/Staff/Admin] Start work on work order
    ///
    /// **Business Rules:**
    /// - Must be assigned to this work order (or Admin/Staff can start on behalf)
    /// - Must be on-shift (checked in) or have valid schedule
    /// - Work order must be in Assigned or Created status
    ///
    /// **On-Shift Validation:**
    /// System checks if technician has checked in for shift today or has valid schedule.
    /// Can be skipped with skipShiftValidation=true for Admin/Staff.
    /// </summary>
    [HttpPost("{id:int}/start")]
    [Authorize(Policy = "AllInternal")]  // Allow Admin, Staff, Technician
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
    /// [Technician/Staff/Admin] Complete work order
    ///
    /// **Business Rules:**
    /// - Must be assigned to this work order (or Admin/Staff can complete on behalf)
    /// - Work order must be InProgress
    /// - All required checklist items must be completed
    ///
    /// **Post-Completion:**
    /// - Auto-generates invoice
    /// - Creates maintenance history
    /// - Updates appointment status
    /// </summary>
    [HttpPost("{id:int}/complete")]
    [Authorize(Policy = "AllInternal")]  // Allow Admin, Staff, Technician
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

    #region Delivery Validation (ISSUE #1 FIX - Point 3)

    /// <summary>
    /// [ISSUE #1 FIX] Validate if work order can be delivered to customer
    ///
    /// **Business Rules:**
    /// - WorkOrder must be Completed
    /// - Invoice must be fully paid (OutstandingAmount = 0)
    /// - Quality check must be completed (if required)
    ///
    /// **Use Case:**
    /// Staff calls this API before allowing customer to pick up vehicle.
    /// Frontend shows payment reminder if invoice is not fully paid.
    ///
    /// **Response:**
    /// ```json
    /// {
    ///   "canDeliver": true/false,
    ///   "reason": "error message if cannot deliver",
    ///   "outstandingAmount": 0,
    ///   "invoiceCode": "INV-...",
    ///   "message": "success message"
    /// }
    /// ```
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Delivery validation result</returns>
    [HttpGet("{id:int}/validate-delivery")]
    [Authorize(Policy = "AllInternal")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateDelivery(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();

            _logger.LogInformation(
                "Validating delivery for WorkOrder {WorkOrderId} by user {UserId}",
                id, userId);

            var workOrder = await _workOrderService.GetWorkOrderAsync(id, cancellationToken);

            if (workOrder == null)
            {
                return NotFoundError($"Work order {id} not found");
            }

            // ✅ Check 1: WorkOrder must be Completed
            if (!string.Equals(workOrder.StatusName, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                return Success(new
                {
                    CanDeliver = false,
                    Reason = $"WorkOrder chưa hoàn thành. Trạng thái hiện tại: {workOrder.StatusName ?? "Unknown"}",
                    WorkOrderCode = workOrder.WorkOrderCode,
                    CurrentStatus = workOrder.StatusName
                });
            }

            // ✅ Check outstanding payment (use appointment snapshot fields)
            var outstandingAmount = workOrder.AppointmentOutstandingAmount ?? 0m;

            if (outstandingAmount > 0)
            {
                _logger.LogWarning(
                    "WorkOrder {WorkOrderCode} cannot be delivered. Outstanding payment: {Outstanding}đ",
                    workOrder.WorkOrderCode, outstandingAmount);

                // ✅ Get invoice information for payment endpoints
                InvoiceResponseDto? invoice = null;
                try
                {
                    invoice = await _invoiceService.GetInvoiceByWorkOrderIdAsync(id, cancellationToken);
                }
                catch
                {
                    // service may throw if not found; silently continue
                }

                var totalAmount = workOrder.AppointmentFinalCost ?? workOrder.AppointmentEstimatedCost ?? 0m;

                return Success(new
                {
                    CanDeliver = false,
                    Reason = $"⚠️ Còn công nợ {outstandingAmount:N0}đ. Vui lòng thanh toán trước khi giao xe.",
                    WorkOrderCode = workOrder.WorkOrderCode,
                    InvoiceCode = invoice?.InvoiceCode, // ✅ For frontend display
                    InvoiceId = invoice?.InvoiceId, // ✅ Required for /api/payments and /api/payments/manual
                    AppointmentId = workOrder.AppointmentId, // ✅ For payment intent creation
                    OutstandingAmount = outstandingAmount,
                    TotalAmount = totalAmount,
                    PaidAmount = totalAmount - outstandingAmount,
                    CreatePaymentIntentUrl = workOrder.AppointmentId.HasValue
                        ? $"/api/appointments/{workOrder.AppointmentId}/payments/create-intent"
                        : null, // ✅ Shortcut for staff: "pay outstanding for this appointment"
                    Message = "Khách hàng cần thanh toán trước khi nhận xe"
                });
            }

            // ✅ Check 3: Quality check (if required)
            if (workOrder.QualityCheckRequired == true && !workOrder.QualityCheckedBy.HasValue)
            {
                return Success(new
                {
                    CanDeliver = false,
                    Reason = "Chưa qua kiểm tra chất lượng. Vui lòng thực hiện quality check trước khi giao xe.",
                    WorkOrderCode = workOrder.WorkOrderCode,
                    InvoiceCode = (string?)null,
                    QualityCheckRequired = true,
                    QualityCheckCompleted = false
                });
            }

            // ✅ All checks passed - can deliver
            _logger.LogInformation(
                "✅ WorkOrder {WorkOrderCode} ready for delivery. Invoice {InvoiceCode} fully paid.",
                workOrder.WorkOrderCode, "N/A");

            return Success(new
            {
                CanDeliver = true,
                Reason = (string?)null,
                WorkOrderCode = workOrder.WorkOrderCode,
                InvoiceCode = (string?)null,
                PaidAmount = (workOrder.AppointmentFinalCost ?? workOrder.AppointmentEstimatedCost ?? 0m) - (workOrder.AppointmentOutstandingAmount ?? 0m),
                OutstandingAmount = 0m,
                QualityCheckCompleted = workOrder.QualityCheckedBy.HasValue,
                QualityRating = workOrder.QualityRating,
                Message = "✅ Xe đã sẵn sàng giao cho khách hàng"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating delivery for work order {WorkOrderId}", id);
            return ServerError("Lỗi khi kiểm tra điều kiện giao xe");
        }
    }

    #endregion
}
