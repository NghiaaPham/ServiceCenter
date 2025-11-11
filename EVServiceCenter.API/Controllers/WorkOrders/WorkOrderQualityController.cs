using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EVServiceCenter.API.Controllers;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;

namespace EVServiceCenter.API.Controllers.WorkOrders
{
    /// <summary>
    /// Controller for Work Order Quality Check operations
    /// </summary>
    [ApiController]
    [Route("api/work-orders")]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "Staff - Work Orders")]
    public class WorkOrderQualityController : BaseController
    {
        private readonly EVDbContext _context;
        private readonly ILogger<WorkOrderQualityController> _logger;

        public WorkOrderQualityController(
            EVDbContext context,
            ILogger<WorkOrderQualityController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Check if current user can perform quality check on a work order
        /// </summary>
        private async Task<bool> CanPerformQualityCheck(WorkOrder workOrder, int currentUserId)
        {
            // Admin always has permission
            if (IsAdmin())
            {
                _logger.LogDebug("User {UserId} is Admin - quality check allowed", currentUserId);
                return true;
            }

            // Staff who is the advisor of this work order can perform quality check
            if (IsStaff() && workOrder.AdvisorId == currentUserId)
            {
                _logger.LogDebug("User {UserId} is Advisor - quality check allowed", currentUserId);
                return true;
            }

            // Manager of the service center can perform quality check
            var serviceCenter = await _context.ServiceCenters
                .FirstOrDefaultAsync(sc => sc.CenterId == workOrder.ServiceCenterId);

            if (serviceCenter?.ManagerId == currentUserId)
            {
                _logger.LogDebug("User {UserId} is Service Center Manager - quality check allowed", currentUserId);
                return true;
            }

            _logger.LogWarning("User {UserId} does not have permission for quality check on work order {WorkOrderId}", 
                currentUserId, workOrder.WorkOrderId);
            return false;
        }

        /// <summary>
        /// Perform quality check on a completed work order
        /// </summary>
        /// <remarks>
        /// Only Admin, Advisor (Staff), or Service Center Manager can perform quality check.
        /// Work order must be in "Completed" status.
        /// </remarks>
        [HttpPost("{workOrderId}/quality-check")]
        [Authorize(Policy = "AllInternal")]
        [ProducesResponseType(typeof(QualityCheckResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> PerformQualityCheck(
            int workOrderId,
            [FromBody] QualityCheckRequestDto request)
        {
            var currentUserId = GetCurrentUserId();
            var currentUserName = GetCurrentUserName();

            _logger.LogInformation(
                "User {UserId} ({UserName}) attempting quality check for work order {WorkOrderId}",
                currentUserId, currentUserName, workOrderId);

            // Validate request
            if (!ModelState.IsValid)
            {
                return ValidationError("Invalid quality check data");
            }

            // Get work order with related data
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.Status)
                .Include(wo => wo.ServiceCenter)
                .Include(wo => wo.QualityCheckedByNavigation)
                .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId);

            if (workOrder == null)
            {
                _logger.LogWarning("Work order {WorkOrderId} not found", workOrderId);
                return NotFoundError("Work order not found");
            }

            // Check permission
            if (!await CanPerformQualityCheck(workOrder, currentUserId))
            {
                return ForbiddenError(
                    "You don't have permission to perform quality check for this work order. " +
                    "Only Admin, Advisor, or Service Center Manager can perform quality checks.");
            }

            // Check work order status
            if (workOrder.Status?.StatusName != "Completed")
            {
                return ValidationError(
                    $"Work order must be in 'Completed' status before quality check. Current status: {workOrder.Status?.StatusName ?? "Unknown"}");
            }

            // Check if already quality checked
            if (workOrder.QualityCheckedBy.HasValue)
            {
                return ValidationError(
                    $"Quality check has already been performed by {workOrder.QualityCheckedByNavigation?.FullName ?? "Unknown"} " +
                    $"on {workOrder.QualityCheckDate?.ToString("yyyy-MM-dd HH:mm:ss")}");
            }

            // Perform quality check
            workOrder.QualityCheckedBy = currentUserId;
            workOrder.QualityCheckDate = DateTime.UtcNow;
            workOrder.QualityRating = request.Rating;

            // Add quality check notes to internal notes
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                var qualityNotes = $"\n[Quality Check - {DateTime.UtcNow:yyyy-MM-dd HH:mm}] Rating: {request.Rating}/5\n{request.Notes}";
                workOrder.InternalNotes = string.IsNullOrEmpty(workOrder.InternalNotes)
                    ? qualityNotes.TrimStart()
                    : workOrder.InternalNotes + qualityNotes;
            }

            // Auto-assign supervisor if not already set
            if (!workOrder.SupervisorId.HasValue)
            {
                workOrder.SupervisorId = currentUserId;
                _logger.LogInformation(
                    "Auto-assigned supervisor {UserId} to work order {WorkOrderId}",
                    currentUserId, workOrderId);
            }

            try
            {
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Quality check completed successfully for work order {WorkOrderId} by user {UserId}. Rating: {Rating}/5",
                    workOrderId, currentUserId, request.Rating);

                var response = new QualityCheckResponseDto
                {
                    WorkOrderId = workOrder.WorkOrderId,
                    WorkOrderCode = workOrder.WorkOrderCode,
                    QualityCheckedBy = currentUserId,
                    QualityCheckedByName = currentUserName,
                    QualityCheckDate = workOrder.QualityCheckDate!.Value,
                    QualityRating = workOrder.QualityRating!.Value,
                    SupervisorId = workOrder.SupervisorId,
                    Message = "Quality check completed successfully"
                };

                return Success(response, "Quality check completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing quality check for work order {WorkOrderId}", workOrderId);
                return ServerError("Failed to save quality check. Please try again.");
            }
        }

        /// <summary>
        /// Check if a work order can be rated by customer
        /// </summary>
        /// <remarks>
        /// Returns whether the work order is eligible for customer rating.
        /// Work order must be completed and quality checked (if required).
        /// **PUBLIC ENDPOINT** - AllowAnonymous for customers to check without login.
        /// </remarks>
        [HttpGet("{workOrderId}/can-rate")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(CanRateResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CanRateWorkOrder(int workOrderId)
        {
            _logger.LogDebug("Checking if work order {WorkOrderId} can be rated", workOrderId);

            var workOrder = await _context.WorkOrders
                .Include(wo => wo.Status)
                .Include(wo => wo.QualityCheckedByNavigation)
                .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId);

            if (workOrder == null)
            {
                _logger.LogWarning("Work order {WorkOrderId} not found for can-rate check", workOrderId);
                return NotFoundError("Work order not found");
            }

            var canRate = true;
            string? reason = null;

            // Check 1: Work order must be completed
            if (workOrder.Status?.StatusName != "Completed")
            {
                canRate = false;
                reason = $"Work order must be completed before rating. Current status: {workOrder.Status?.StatusName ?? "Unknown"}";
            }
            // Check 2: Quality check must be completed (if required)
            else if (workOrder.QualityCheckRequired == true && !workOrder.QualityCheckedBy.HasValue)
            {
                canRate = false;
                reason = "Quality check must be completed by staff before customer can rate";
            }
            // Check 3: Not already rated
            else
            {
                var hasRating = await _context.ServiceRatings
                    .AnyAsync(sr => sr.WorkOrderId == workOrderId);

                if (hasRating)
                {
                    canRate = false;
                    reason = "This work order has already been rated";
                }
            }

            var response = new CanRateResponseDto
            {
                WorkOrderId = workOrder.WorkOrderId,
                WorkOrderCode = workOrder.WorkOrderCode,
                CanRate = canRate,
                Reason = reason,
                WorkOrderStatus = workOrder.Status?.StatusName,
                CompletedDate = workOrder.CompletedDate,
                QualityCheckRequired = workOrder.QualityCheckRequired ?? false,
                QualityCheckCompleted = workOrder.QualityCheckedBy.HasValue,
                QualityCheckedByName = workOrder.QualityCheckedByNavigation?.FullName,
                QualityCheckDate = workOrder.QualityCheckDate,
                QualityRating = workOrder.QualityRating
            };

            _logger.LogInformation(
                "Can rate check for work order {WorkOrderId}: {CanRate}. Reason: {Reason}",
                workOrderId, canRate, reason ?? "OK");

            return Success(response);
        }

        /// <summary>
        /// Get quality check history for a work order
        /// </summary>
        [HttpGet("{workOrderId}/quality-check")]
        [Authorize(Policy = "AllInternal")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetQualityCheckInfo(int workOrderId)
        {
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.QualityCheckedByNavigation)
                .Include(wo => wo.Supervisor)
                .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId);

            if (workOrder == null)
            {
                return NotFoundError("Work order not found");
            }

            var info = new
            {
                WorkOrderId = workOrder.WorkOrderId,
                WorkOrderCode = workOrder.WorkOrderCode,
                QualityCheckRequired = workOrder.QualityCheckRequired,
                QualityCheckCompleted = workOrder.QualityCheckedBy.HasValue,
                QualityCheckedBy = workOrder.QualityCheckedBy,
                QualityCheckedByName = workOrder.QualityCheckedByNavigation?.FullName,
                QualityCheckDate = workOrder.QualityCheckDate,
                QualityRating = workOrder.QualityRating,
                SupervisorId = workOrder.SupervisorId,
                SupervisorName = workOrder.Supervisor?.FullName,
                InternalNotes = workOrder.InternalNotes
            };

            return Success(info);
        }
    }
}
