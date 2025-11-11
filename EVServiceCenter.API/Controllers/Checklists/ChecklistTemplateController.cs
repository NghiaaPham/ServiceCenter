using EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;
using EVServiceCenter.Core.Domains.Checklists.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Checklists;

/// <summary>
/// Checklist Template Management
/// Handles CRUD operations for checklist templates
/// </summary>
[ApiController]
[Route("api/checklist-templates")]
[ApiExplorerSettings(GroupName = "Checklist Management")]
[Authorize]
public class ChecklistTemplateController : ControllerBase
{
    private readonly IChecklistService _checklistService;
    private readonly ILogger<ChecklistTemplateController> _logger;

    public ChecklistTemplateController(
        IChecklistService checklistService,
        ILogger<ChecklistTemplateController> logger)
    {
        _checklistService = checklistService;
        _logger = logger;
    }

    /// <summary>
    /// [List] Get all checklist templates with filtering and pagination
    /// </summary>
    /// <remarks>
    /// **Filters:**
    /// - SearchTerm: Search by template name
    /// - ServiceId: Filter by service
    /// - CategoryId: Filter by category
    /// - IsActive: Filter by active status
    ///
    /// **Sorting:**
    /// - SortBy: templateName (default), createdDate
    /// - SortDirection: asc (default), desc
    ///
    /// **Response includes:**
    /// - Template details with parsed items
    /// - Service and category names
    /// - Creator information
    /// - Total items count
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(
        [FromQuery] ChecklistTemplateQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _checklistService.GetTemplatesAsync(query, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting checklist templates");
            return StatusCode(500, new { success = false, message = "Error retrieving templates" });
        }
    }

    /// <summary>
    /// [Details] Get checklist template by ID
    /// </summary>
    /// <remarks>
    /// Returns full template details including:
    /// - All checklist items (ordered)
    /// - Service and category information
    /// - Creator details
    /// - Active status
    /// </remarks>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _checklistService.GetTemplateByIdAsync(id, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Template {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template {TemplateId}", id);
            return StatusCode(500, new { success = false, message = "Error retrieving template" });
        }
    }

    /// <summary>
    /// [Create] Create new checklist template
    /// </summary>
    /// <remarks>
    /// **Request body example:**
    /// ```json
    /// {
    ///   "templateName": "Battery Inspection Checklist",
    ///   "serviceId": 1,
    ///   "categoryId": 2,
    ///   "isActive": true,
    ///   "items": [
    ///     {
    ///       "order": 1,
    ///       "description": "Check battery voltage (12.4V - 12.9V)",
    ///       "isRequired": true
    ///     },
    ///     {
    ///       "order": 2,
    ///       "description": "Inspect battery terminals for corrosion",
    ///       "isRequired": true
    ///     },
    ///     {
    ///       "order": 3,
    ///       "description": "Test charging system output",
    ///       "isRequired": false
    ///     }
    ///   ]
    /// }
    /// ```
    ///
    /// **Business Rules:**
    /// - Template name is required (max 100 chars)
    /// - Must have at least 1 item, max 50 items
    /// - Item order numbers must be unique
    /// - Item descriptions max 500 chars
    /// </remarks>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTemplate(
        [FromBody] CreateChecklistTemplateRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _checklistService.CreateTemplateAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetTemplate),
                new { id = result.TemplateId },
                new { success = true, data = result, message = "Template created successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating checklist template");
            return StatusCode(500, new { success = false, message = "Error creating template" });
        }
    }

    /// <summary>
    /// [Update] Update existing checklist template
    /// </summary>
    /// <remarks>
    /// **Updatable fields:**
    /// - TemplateName
    /// - ServiceId
    /// - CategoryId
    /// - Items (replaces all items)
    /// - IsActive
    ///
    /// **Note:** Only provide fields you want to update. Null/omitted fields are not changed.
    /// </remarks>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateTemplate(
        int id,
        [FromBody] UpdateChecklistTemplateRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _checklistService.UpdateTemplateAsync(id, request, cancellationToken);
            return Ok(new { success = true, data = result, message = "Template updated successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Template {id} not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating template {TemplateId}", id);
            return StatusCode(500, new { success = false, message = "Error updating template" });
        }
    }

    /// <summary>
    /// [Delete] Delete checklist template (soft delete)
    /// </summary>
    /// <remarks>
    /// Sets template IsActive = false.
    /// Template is hidden from queries but data is preserved.
    /// </remarks>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTemplate(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _checklistService.DeleteTemplateAsync(id, cancellationToken);

            if (result)
                return NoContent();

            return NotFound(new { success = false, message = $"Template {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template {TemplateId}", id);
            return StatusCode(500, new { success = false, message = "Error deleting template" });
        }
    }

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
