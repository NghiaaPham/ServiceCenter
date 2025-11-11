using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using EVServiceCenter.Core.Domains.Checklists.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Checklists;

/// <summary>
/// Checklist Item Management
/// Handles work order checklist operations and item status updates
/// </summary>
[ApiController]
[Route("api")]
[ApiExplorerSettings(GroupName = "Checklist Management")]
[Authorize]
public class ChecklistItemController : ControllerBase
{
    private readonly IChecklistService _checklistService;
    private readonly ILogger<ChecklistItemController> _logger;

    public ChecklistItemController(
        IChecklistService checklistService,
        ILogger<ChecklistItemController> logger)
    {
        _checklistService = checklistService;
        _logger = logger;
    }

    #region Work Order Checklist Operations

    /// <summary>
    /// [Get] Get work order checklist with completion status
    /// </summary>
    /// <remarks>
    /// Returns complete checklist for work order including:
    /// - All checklist items (ordered)
    /// - Completion status for each item
    /// - Overall completion percentage
    /// - Completed by/date information
    /// - Notes and images
    ///
    /// **Use Case:** Display checklist in work order details screen
    /// </remarks>
    [HttpGet("work-orders/{workOrderId:int}/checklist")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkOrderChecklist(
        int workOrderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _checklistService.GetWorkOrderChecklistAsync(workOrderId, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                success = false,
                message = $"Work order {workOrderId} not found or has no checklist"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting checklist for work order {WorkOrderId}", workOrderId);
            return StatusCode(500, new { success = false, message = "Error retrieving checklist" });
        }
    }

    /// <summary>
    /// [Apply] Apply checklist template to work order
    /// </summary>
    /// <remarks>
    /// Creates checklist items from template for the work order.
    ///
    /// **Request body example:**
    /// ```json
    /// {
    ///   "templateId": 5
    /// }
    /// ```
    ///
    /// **Or with custom items:**
    /// ```json
    /// {
    ///   "templateId": 5,
    ///   "customItems": [
    ///     {
    ///       "order": 1,
    ///       "description": "Custom check for this specific job",
    ///       "isRequired": true
    ///     }
    ///   ]
    /// }
    /// ```
    ///
    /// **Business Rules:**
    /// - Work order must exist
    /// - Template must exist and be active
    /// - If customItems provided, they replace template items
    /// - Creates ChecklistItem records linked to work order
    ///
    /// **Use Case:** When work order is created or started, apply appropriate checklist template
    /// </remarks>
    [HttpPost("work-orders/{workOrderId:int}/apply-checklist")]
    [Authorize(Roles = "Admin,Manager,Staff,Technician")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyChecklistTemplate(
        int workOrderId,
        [FromBody] ApplyChecklistTemplateRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _checklistService.ApplyTemplateToWorkOrderAsync(
                workOrderId, request, cancellationToken);

            return Ok(new
            {
                success = true,
                data = result,
                message = $"Checklist applied successfully ({result.TotalItems} items created)"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying checklist to work order {WorkOrderId}", workOrderId);
            return StatusCode(500, new { success = false, message = "Error applying checklist" });
        }
    }

    #endregion

    #region Checklist Item Status Updates

    /// <summary>
    /// [Update] Update checklist item status, notes, and image
    /// </summary>
    /// <remarks>
    /// **Request body example:**
    /// ```json
    /// {
    ///   "isCompleted": true,
    ///   "notes": "Battery voltage: 12.6V - Good condition",
    ///   "imageUrl": "https://storage.example.com/battery-check.jpg"
    /// }
    /// ```
    ///
    /// **Fields:**
    /// - isCompleted: Mark item as complete/incomplete
    /// - notes: Technician notes/findings (max 500 chars)
    /// - imageUrl: Evidence photo URL (max 500 chars)
    ///
    /// **Business Rules:**
    /// - When marking complete, CompletedBy and CompletedDate are auto-set
    /// - When marking incomplete, CompletedBy and CompletedDate are cleared
    /// - At least one field must be provided
    ///
    /// **Use Case:** Technician updates item status as they work through checklist
    /// </remarks>
    [HttpPut("checklist-items/{itemId:int}")]
    [Authorize(Roles = "Admin,Manager,Staff,Technician")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateChecklistItem(
        int itemId,
        [FromBody] UpdateChecklistItemStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _checklistService.UpdateChecklistItemAsync(
                itemId, request, userId, cancellationToken);

            return Ok(new { success = true, data = result, message = "Checklist item updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Checklist item {itemId} not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating checklist item {ItemId}", itemId);
            return StatusCode(500, new { success = false, message = "Error updating checklist item" });
        }
    }

    /// <summary>
    /// [Complete] Mark checklist item as complete
    /// </summary>
    /// <remarks>
    /// Convenience endpoint for marking items complete.
    ///
    /// **Request body example:**
    /// ```json
    /// {
    ///   "notes": "All checks passed - battery in good condition"
    /// }
    /// ```
    ///
    /// Automatically sets:
    /// - IsCompleted = true
    /// - CompletedBy = current user
    /// - CompletedDate = now
    ///
    /// **Use Case:** Quick action button "Mark Complete" in mobile app
    /// </remarks>
    [HttpPatch("checklist-items/{itemId:int}/complete")]
    [Authorize(Roles = "Admin,Manager,Staff,Technician")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkItemComplete(
        int itemId,
        [FromBody] string? notes,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _checklistService.MarkItemCompleteAsync(itemId, userId, notes, cancellationToken);

            return Ok(new { success = true, data = result, message = "Item marked complete" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Checklist item {itemId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking item {ItemId} complete", itemId);
            return StatusCode(500, new { success = false, message = "Error marking item complete" });
        }
    }

    /// <summary>
    /// [Uncomplete] Mark checklist item as incomplete
    /// </summary>
    /// <remarks>
    /// Resets completion status:
    /// - IsCompleted = false
    /// - CompletedBy = null
    /// - CompletedDate = null
    ///
    /// **Use Case:** Undo accidental completion or re-open item for re-inspection
    /// </remarks>
    [HttpPatch("checklist-items/{itemId:int}/uncomplete")]
    [Authorize(Roles = "Admin,Manager,Staff,Technician")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkItemIncomplete(
        int itemId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _checklistService.MarkItemIncompleteAsync(itemId, cancellationToken);

            return Ok(new { success = true, data = result, message = "Item marked incomplete" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Checklist item {itemId} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking item {ItemId} incomplete", itemId);
            return StatusCode(500, new { success = false, message = "Error marking item incomplete" });
        }
    }

    #endregion

    #region NEW API Endpoints: Complete/Skip/Validate/BulkComplete

    /// <summary>
    /// [Complete Item] Complete m?t checklist item v?i validation
    /// </summary>
    /// <remarks>
    /// **Request body example:**
    /// ```json
    /// {
    ///   "itemId": 123,
    ///   "workOrderId": 45,
    ///   "notes": "Battery voltage: 12.6V - Good condition",
    ///   "imageUrl": "https://storage.example.com/battery-check.jpg"
    /// }
    /// ```
    ///
    /// **Business Rules:**
    /// - ItemId ph?i thu?c v? WorkOrderId
    /// - Auto-update WorkOrder progress sau khi complete
    /// - CompletedBy = current user
    /// - CompletedDate = now
    ///
    /// **Use Case:** Technician complete t?ng item khi làm vi?c
    /// </remarks>
    [HttpPost("items/complete")]
    [Authorize(Roles = "Admin,Manager,Staff,Technician")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteItem(
        [FromBody] CompleteChecklistItemRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _checklistService.CompleteChecklistItemAsync(
                request, userId, cancellationToken);

            return Ok(new
            {
                success = true,
                data = result,
                message = "Checklist item completed successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing checklist item");
            return StatusCode(500, new { success = false, message = "Error completing checklist item" });
        }
    }

    /// <summary>
    /// [Skip Item] Skip m?t checklist item v?i lý do (ch? cho optional items)
    /// </summary>
    /// <remarks>
    /// **Request body example:**
    /// ```json
    /// {
    ///   "itemId": 124,
    ///   "workOrderId": 45,
    ///   "skipReason": "Khách hàng t? ch?i - không mu?n ki?m tra m?c này"
    /// }
    /// ```
    ///
    /// **Business Rules:**
    /// - CH? skip ???c optional items (IsRequired = false)
    /// - Required items KHÔNG ???c skip
    /// - SkipReason b?t bu?c (10-500 ký t?)
    /// - Item ???c mark completed v?i notes = "[SKIPPED] {reason}"
    ///
    /// **Use Case:** Technician skip optional items khi không áp d?ng
    /// </remarks>
    [HttpPost("items/skip")]
    [Authorize(Roles = "Admin,Manager,Staff,Technician")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SkipItem(
        [FromBody] SkipChecklistItemRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _checklistService.SkipChecklistItemAsync(
                request, userId, cancellationToken);

            return Ok(new
            {
                success = true,
                data = result,
                message = "Checklist item skipped successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error skipping checklist item");
            return StatusCode(500, new { success = false, message = "Error skipping checklist item" });
        }
    }

    /// <summary>
    /// [Validate] Validate xem WorkOrder có th? complete không
    /// </summary>
    /// <remarks>
    /// Ki?m tra t?t c? required items ?ã completed ch?a.
    ///
    /// **Response example:**
    /// ```json
    /// {
    ///   "canComplete": false,
    ///   "missingItems": [
    ///     "Ki?m tra ?i?n áp pin",
    ///     "Ki?m tra h? th?ng phanh"
    ///   ]
    /// }
    /// ```
    ///
    /// **Business Rules:**
    /// - CanComplete = true n?u T?T C? required items ?ã completed
    /// - MissingItems = danh sách tên các required items ch?a done
    ///
    /// **Use Case:**
    /// - Before calling CompleteWorkOrderAsync()
    /// - Show warning UI: "Còn 3 items ch?a xong"
    /// - Block complete button n?u CanComplete = false
    /// </remarks>
    [HttpGet("work-orders/{workOrderId:int}/validate")]
    [Authorize(Roles = "Admin,Manager,Staff,Technician")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateWorkOrderCompletion(
        int workOrderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var (canComplete, missingItems) = await _checklistService.ValidateWorkOrderCompletionAsync(
                workOrderId, cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    workOrderId,
                    canComplete,
                    missingItems,
                    missingCount = missingItems.Count
                },
                message = canComplete 
                    ? "WorkOrder ready to complete" 
                    : $"WorkOrder cannot complete: {missingItems.Count} required items missing"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating work order completion");
            return StatusCode(500, new { success = false, message = "Error validating work order" });
        }
    }

    /// <summary>
    /// [Bulk Complete] Complete T?T C? checklist items c?a WorkOrder
    /// </summary>
    /// <remarks>
    /// **Request body example:**
    /// ```json
    /// {
    ///   "workOrderId": 45,
    ///   "notes": "Bulk completed after full inspection"
    /// }
    /// ```
    ///
    /// **Business Rules:**
    /// - Complete t?t c? incomplete items
    /// - Dùng same notes cho t?t c? items
    /// - Auto-update WorkOrder progress
    /// - Return s? items succeeded/failed
    ///
    /// **Use Case:**
    /// - Quick complete toàn b? checklist
    /// - Testing purpose
    /// - Skip manual tick t?ng item
    ///
    /// **Warning:** Ch? dùng khi ch?c ch?n t?t c? items ?ã OK
    /// </remarks>
    [HttpPost("work-orders/{workOrderId:int}/complete-all")]
    [Authorize(Roles = "Admin,Manager,Staff,Technician")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CompleteAllItems(
        int workOrderId,
        [FromBody] BulkCompleteRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _checklistService.CompleteAllItemsAsync(
                workOrderId,
                request.Notes,
                userId,
                cancellationToken);

            var message = result.FailedItems > 0
                ? $"Completed {result.CompletedItems}/{result.TotalItems} items. {result.FailedItems} failed."
                : $"All {result.CompletedItems} items completed successfully";

            return Ok(new
            {
                success = true,
                data = result,
                message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk completing checklist items");
            return StatusCode(500, new { success = false, message = "Error bulk completing items" });
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
