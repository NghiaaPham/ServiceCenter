using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.WorkOrders;

/// <summary>
/// Work Order Timeline & Audit Trail
/// Handles timeline events, notes, and activity tracking
/// </summary>
[ApiController]
[Route("api/work-orders")]
[ApiExplorerSettings(GroupName = "WorkOrder Timeline")]
[Authorize]
public class WorkOrderTimelineController : ControllerBase
{
    private readonly IWorkOrderTimelineService _timelineService;
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly ILogger<WorkOrderTimelineController> _logger;

    public WorkOrderTimelineController(
        IWorkOrderTimelineService timelineService,
        IWorkOrderRepository workOrderRepository,
        ILogger<WorkOrderTimelineController> logger)
    {
        _timelineService = timelineService;
        _workOrderRepository = workOrderRepository;
        _logger = logger;
    }

    #region Timeline Query

    /// <summary>
    /// Get complete timeline of work order events
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="includeInternal">Include internal/hidden events (default: false)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of timeline events ordered by date</returns>
    [HttpGet("{id:int}/timeline")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTimeline(
        int id,
        [FromQuery] bool includeInternal = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var timeline = await _timelineService.GetTimelineAsync(id, includeInternal, cancellationToken);
            return Ok(new { success = true, data = timeline });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting timeline for work order {WorkOrderId}", id);
            return StatusCode(500, new { success = false, message = "Error retrieving timeline" });
        }
    }

    /// <summary>
    /// Get customer-visible timeline events only
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer-visible events</returns>
    [HttpGet("{id:int}/timeline/customer")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerTimeline(
        int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // SECURITY: Verify ownership for customer role
            if (!await VerifyWorkOrderOwnershipAsync(id, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to access timeline for work order {WorkOrderId} without ownership",
                    GetCurrentUserId(), id);
                return Forbid();
            }

            var timeline = await _timelineService.GetCustomerTimelineAsync(id, cancellationToken);
            return Ok(new { success = true, data = timeline });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer timeline for work order {WorkOrderId}", id);
            return StatusCode(500, new { success = false, message = "Error retrieving customer timeline" });
        }
    }

    #endregion

    #region Timeline Events

    /// <summary>
    /// Add timeline event to work order
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="request">Timeline event request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created timeline event</returns>
    [HttpPost("{id:int}/timeline")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddTimelineEvent(
        int id,
        [FromBody] AddWorkOrderTimelineRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _timelineService.AddTimelineEventAsync(
                id,
                request,
                userId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetTimeline),
                new { id },
                new { success = true, data = result, message = "Timeline event added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding timeline event to work order {WorkOrderId}", id);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Add customer note to timeline (visible to customer)
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="note">Note content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created timeline event</returns>
    [HttpPost("{id:int}/timeline/customer-note")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddCustomerNote(
        int id,
        [FromBody] string note,
        CancellationToken cancellationToken)
    {
        try
        {
            // SECURITY: Verify ownership for customer role
            if (!await VerifyWorkOrderOwnershipAsync(id, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to add note to work order {WorkOrderId} without ownership",
                    GetCurrentUserId(), id);
                return Forbid();
            }

            var userId = GetCurrentUserId();
            var request = new AddWorkOrderTimelineRequestDto
            {
                EventType = "CustomerNote",
                EventDescription = note,
                IsVisible = true
            };

            var result = await _timelineService.AddTimelineEventAsync(
                id,
                request,
                userId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetTimeline),
                new { id },
                new { success = true, data = result, message = "Customer note added successfully" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding customer note to work order {WorkOrderId}", id);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Add internal note to timeline (hidden from customer)
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="note">Note content</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created timeline event</returns>
    [HttpPost("{id:int}/timeline/internal-note")]
    [Authorize(Roles = "Admin,Manager,Staff,Technician")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddInternalNote(
        int id,
        [FromBody] string note,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var request = new AddWorkOrderTimelineRequestDto
            {
                EventType = "InternalNote",
                EventDescription = note,
                IsVisible = false
            };

            var result = await _timelineService.AddTimelineEventAsync(
                id,
                request,
                userId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetTimeline),
                new { id },
                new { success = true, data = result, message = "Internal note added successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding internal note to work order {WorkOrderId}", id);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Helper Methods

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("UserId") ?? User.FindFirst("sub");
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token");
    }

    /// <summary>
    /// Verify that the current user owns the work order
    /// Staff/Admin/Manager/Technician bypass this check (via role)
    /// Customers must own the work order
    /// </summary>
    private async Task<bool> VerifyWorkOrderOwnershipAsync(int workOrderId, CancellationToken cancellationToken)
    {
        // Staff and above can access any work order
        if (User.IsInRole("Admin") || User.IsInRole("Manager") ||
            User.IsInRole("Staff") || User.IsInRole("Technician"))
        {
            return true;
        }

        // For customers, verify ownership via CustomerId
        var customerIdClaim = User.FindFirst("CustomerId");
        if (customerIdClaim == null || !int.TryParse(customerIdClaim.Value, out int customerId))
        {
            _logger.LogWarning("CustomerId claim not found for user {UserId}", GetCurrentUserId());
            return false;
        }

        // Get work order and verify CustomerId matches
        var workOrder = await _workOrderRepository.GetByIdAsync(workOrderId, cancellationToken);
        if (workOrder == null)
        {
            throw new KeyNotFoundException($"Work order {workOrderId} not found");
        }

        return workOrder.CustomerId == customerId;
    }

    #endregion
}
