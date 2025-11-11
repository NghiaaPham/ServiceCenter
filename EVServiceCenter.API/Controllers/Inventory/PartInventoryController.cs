using EVServiceCenter.Core.Domains.InventoryManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.InventoryManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Inventory;

/// <summary>
/// Part Inventory Query & Management
/// Handles inventory lookup, low stock alerts, and stock reservations
/// </summary>
[ApiController]
[Route("api/inventory")]
[ApiExplorerSettings(GroupName = "Inventory Management")]
[Authorize]
public class PartInventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<PartInventoryController> _logger;

    public PartInventoryController(
        IInventoryService inventoryService,
        ILogger<PartInventoryController> logger)
    {
        _inventoryService = inventoryService;
        _logger = logger;
    }

    /// <summary>
    /// [List] Get part inventory with filtering and pagination
    /// </summary>
    /// <remarks>
    /// **Filters:**
    /// - ServiceCenterId: Filter by service center
    /// - CategoryId: Filter by part category
    /// - SupplierId: Filter by supplier
    /// - IsLowStock: Show only low stock items
    /// - IsOutOfStock: Show only out of stock items
    /// - SearchTerm: Search by part code, name, or barcode
    ///
    /// **Sorting:**
    /// - SortBy: partCode, partName, currentStock, lastUpdated
    /// - SortDirection: asc, desc
    ///
    /// **Response includes:**
    /// - Current stock, reserved stock, available stock
    /// - Stock status (OK, LOW, OUT_OF_STOCK, OVERSTOCKED)
    /// - Reorder recommendations
    /// - Stock value calculations
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventory(
        [FromQuery] PartInventoryQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _inventoryService.GetInventoryAsync(query, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory");
            return StatusCode(500, new { success = false, message = "Error retrieving inventory" });
        }
    }

    /// <summary>
    /// [Details] Get inventory for specific part at service center
    /// </summary>
    [HttpGet("part/{partId}/center/{serviceCenterId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInventoryByPartAndCenter(
        int partId,
        int serviceCenterId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _inventoryService.GetInventoryByPartAndCenterAsync(
                partId, serviceCenterId, cancellationToken);

            if (result == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = $"No inventory found for Part {partId} at Service Center {serviceCenterId}"
                });
            }

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory for Part {PartId} at Center {CenterId}",
                partId, serviceCenterId);
            return StatusCode(500, new { success = false, message = "Error retrieving inventory" });
        }
    }

    /// <summary>
    /// [Alerts] Get low stock alerts with priority sorting
    /// </summary>
    /// <remarks>
    /// Returns parts that need reordering, sorted by criticality:
    ///
    /// **Alert Levels:**
    /// - CRITICAL: Out of stock (0 units)
    /// - HIGH: Below 25% of reorder level
    /// - MEDIUM: Below 50% of reorder level
    /// - LOW: Below 100% of reorder level
    ///
    /// **Response includes:**
    /// - Stock shortage amount
    /// - Suggested order quantity
    /// - Preferred supplier information
    /// - Average monthly usage (if available)
    /// - Estimated days until stockout
    ///
    /// **Use Cases:**
    /// - Purchase planning
    /// - Inventory manager dashboard
    /// - Automated reorder triggers
    /// </remarks>
    [HttpGet("low-stock-alerts")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLowStockAlerts(
        [FromQuery] int? serviceCenterId,
        CancellationToken cancellationToken)
    {
        try
        {
            var alerts = await _inventoryService.GetLowStockAlertsAsync(serviceCenterId, cancellationToken);

            var summary = new
            {
                totalAlerts = alerts.Count,
                critical = alerts.Count(a => a.AlertLevel == "CRITICAL"),
                high = alerts.Count(a => a.AlertLevel == "HIGH"),
                medium = alerts.Count(a => a.AlertLevel == "MEDIUM"),
                low = alerts.Count(a => a.AlertLevel == "LOW")
            };

            return Ok(new
            {
                success = true,
                data = alerts,
                summary
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock alerts");
            return StatusCode(500, new { success = false, message = "Error retrieving low stock alerts" });
        }
    }

    /// <summary>
    /// [Analytics] Get total stock value for financial reporting
    /// </summary>
    /// <remarks>
    /// Calculates total inventory value (CurrentStock × CostPrice) across all parts.
    ///
    /// **Cached:** 5 minutes for performance
    ///
    /// **Use Cases:**
    /// - Balance sheet reporting
    /// - Financial dashboards
    /// - Inventory audit
    /// </remarks>
    [HttpGet("total-value")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTotalStockValue(
        [FromQuery] int? serviceCenterId,
        CancellationToken cancellationToken)
    {
        try
        {
            var totalValue = await _inventoryService.GetTotalStockValueAsync(serviceCenterId, cancellationToken);

            return Ok(new
            {
                success = true,
                data = new
                {
                    totalValue,
                    serviceCenterId,
                    currency = "VND",
                    calculatedAt = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total stock value");
            return StatusCode(500, new { success = false, message = "Error calculating stock value" });
        }
    }

    /// <summary>
    /// [Reserve] Reserve stock for work order
    /// </summary>
    /// <remarks>
    /// Reserves stock for a work order (decrements AvailableStock, increments ReservedStock).
    ///
    /// **Business Rules:**
    /// - Validates AvailableStock >= quantity
    /// - Atomic operation with transaction
    /// - Does NOT decrement CurrentStock (only when consumed)
    ///
    /// **Use Case:**
    /// When work order is created/confirmed, reserve parts before actual usage.
    /// </remarks>
    [HttpPost("reserve")]
    [Authorize(Roles = "Admin,Staff,Technician")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReserveStock(
        [FromBody] ReserveStockRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _inventoryService.ReserveStockAsync(
                request.PartId,
                request.ServiceCenterId,
                request.Quantity,
                userId,
                cancellationToken);

            if (!result)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to reserve stock. Insufficient available stock or inventory not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Reserved {request.Quantity} units of Part {request.PartId}"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock");
            return StatusCode(500, new { success = false, message = "Error reserving stock" });
        }
    }

    /// <summary>
    /// [Release] Release reserved stock (cancel reservation)
    /// </summary>
    /// <remarks>
    /// Releases previously reserved stock back to available stock.
    ///
    /// **Use Cases:**
    /// - Work order cancelled
    /// - Part substitution
    /// - Return unused parts
    /// </remarks>
    [HttpPost("release")]
    [Authorize(Roles = "Admin,Staff,Technician")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReleaseReservedStock(
        [FromBody] ReserveStockRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _inventoryService.ReleaseReservedStockAsync(
                request.PartId,
                request.ServiceCenterId,
                request.Quantity,
                userId,
                cancellationToken);

            if (!result)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Failed to release stock. Insufficient reserved stock or inventory not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = $"Released {request.Quantity} units of Part {request.PartId}"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing reserved stock");
            return StatusCode(500, new { success = false, message = "Error releasing stock" });
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

/// <summary>
/// DTO for reserve/release stock operations
/// </summary>
public class ReserveStockRequestDto
{
    public int PartId { get; set; }
    public int ServiceCenterId { get; set; }
    public int Quantity { get; set; }
}
