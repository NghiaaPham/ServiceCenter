using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.WorkOrders;

/// <summary>
/// Work Order Management - CRUD Operations
/// Handles basic Create, Read, Update, Delete operations for work orders
/// </summary>
[ApiController]
[Route("api/work-orders")]
[ApiExplorerSettings(GroupName = "WorkOrder Management")]
[Authorize]
public class WorkOrderManagementController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;
    private readonly ILogger<WorkOrderManagementController> _logger;

    public WorkOrderManagementController(
        IWorkOrderService workOrderService,
        ILogger<WorkOrderManagementController> logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;
    }

    #region Query Operations

    /// <summary>
    /// Get work orders with filtering, sorting, and pagination
    /// </summary>
    /// <param name="query">Query parameters for filtering and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of work orders</returns>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkOrders(
        [FromQuery] WorkOrderQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _workOrderService.GetWorkOrdersAsync(query, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work orders");
            return StatusCode(500, new { success = false, message = "Error retrieving work orders" });
        }
    }

    /// <summary>
    /// Get work order by ID with full details
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed work order information</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkOrder(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _workOrderService.GetWorkOrderAsync(id, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Work order {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work order {WorkOrderId}", id);
            return StatusCode(500, new { success = false, message = "Error retrieving work order" });
        }
    }

    /// <summary>
    /// Get work order by unique code
    /// </summary>
    /// <param name="code">Work order code (e.g., WO-20251021-0001)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Work order details</returns>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkOrderByCode(string code, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _workOrderService.GetWorkOrderByCodeAsync(code, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Work order {code} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work order by code {Code}", code);
            return StatusCode(500, new { success = false, message = "Error retrieving work order" });
        }
    }

    #endregion

    #region Create Operations

    /// <summary>
    /// Create a new work order
    /// </summary>
    /// <param name="request">Work order creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created work order with full details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWorkOrder(
        [FromBody] CreateWorkOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workOrderService.CreateWorkOrderAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetWorkOrder),
                new { id = result.WorkOrderId },
                new { success = true, data = result, message = "Work order created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating work order");
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// ❌ REMOVED: Create work order from existing appointment
    ///
    /// This endpoint has been REMOVED to prevent duplicate WorkOrders.
    ///
    /// **Replacement:**
    /// Use POST /api/appointments/{id}/check-in instead.
    /// Check-in automatically creates WorkOrder with SourceType="Scheduled"
    ///
    /// **Reason for removal:**
    /// - Prevents duplicate WorkOrder creation
    /// - Ensures single source of truth: check-in = WorkOrder creation
    /// - Better business logic: check-in means customer arrived and work starts
    /// </summary>
    // [HttpPost("from-appointment/{appointmentId:int}")]
    // REMOVED - Use appointment check-in instead

    #endregion

    #region Update Operations

    /// <summary>
    /// Update work order details
    /// </summary>
    /// <param name="id">Work order ID</param>
    /// <param name="request">Update request with changes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated work order</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWorkOrder(
        int id,
        [FromBody] UpdateWorkOrderRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workOrderService.UpdateWorkOrderAsync(id, request, userId, cancellationToken);
            return Ok(new { success = true, data = result, message = "Work order updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Work order {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work order {WorkOrderId}", id);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Delete Operations

    /// <summary>
    /// Delete work order (soft delete)
    /// </summary>
    /// <param name="id">Work order ID to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkOrder(int id, CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _workOrderService.DeleteWorkOrderAsync(id, userId, cancellationToken);

            if (result)
                return NoContent();

            return NotFound(new { success = false, message = $"Work order {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting work order {WorkOrderId}", id);
            return StatusCode(500, new { success = false, message = "Error deleting work order" });
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

    #endregion
}
