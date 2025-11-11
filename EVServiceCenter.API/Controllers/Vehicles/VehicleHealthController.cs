using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Vehicles;

/// <summary>
/// Vehicle Health Monitoring (EV-Specific)
/// Handles battery health, motor efficiency, and diagnostic metrics
/// </summary>
[ApiController]
[Route("api/vehicles/health")]
[ApiExplorerSettings(GroupName = "Vehicle Health")]
[Authorize]
public class VehicleHealthController : ControllerBase
{
    private readonly IVehicleHealthService _vehicleHealthService;
    private readonly ICustomerVehicleRepository _vehicleRepository;
    private readonly ILogger<VehicleHealthController> _logger;

    public VehicleHealthController(
        IVehicleHealthService vehicleHealthService,
        ICustomerVehicleRepository vehicleRepository,
        ILogger<VehicleHealthController> logger)
    {
        _vehicleHealthService = vehicleHealthService;
        _vehicleRepository = vehicleRepository;
        _logger = logger;
    }

    #region Record Metrics

    /// <summary>
    /// Record vehicle health metrics after service
    /// </summary>
    /// <param name="request">Health metrics data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Recorded health metrics with auto-calculated overall condition</returns>
    /// <remarks>
    /// Overall condition is automatically calculated as weighted average:
    /// - Battery Health: 40%
    /// - Motor Efficiency: 30%
    /// - Brake Condition: 15%
    /// - Tire Condition: 15%
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> RecordHealthMetric(
        [FromBody] RecordVehicleHealthMetricRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _vehicleHealthService.RecordHealthMetricAsync(request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(GetLatestHealthMetric),
                new { vehicleId = request.VehicleId },
                new { success = true, data = result, message = "Health metrics recorded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording health metrics for Vehicle {VehicleId}", request.VehicleId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Query Metrics

    /// <summary>
    /// Get latest health metrics for specific vehicle
    /// </summary>
    /// <param name="vehicleId">Vehicle ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Most recent health metrics</returns>
    [HttpGet("{vehicleId:int}/latest")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestHealthMetric(int vehicleId, CancellationToken cancellationToken)
    {
        try
        {
            // SECURITY: Verify vehicle ownership for customer role
            if (!await VerifyVehicleOwnershipAsync(vehicleId, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to access health metrics for vehicle {VehicleId} without ownership",
                    GetCurrentUserId(), vehicleId);
                return Forbid();
            }

            var result = await _vehicleHealthService.GetLatestHealthMetricAsync(vehicleId, cancellationToken);
            if (result == null)
                return NotFound(new { success = false, message = $"No health metrics found for vehicle {vehicleId}" });

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest health metric for Vehicle {VehicleId}", vehicleId);
            return StatusCode(500, new { success = false, message = "Error retrieving health metrics" });
        }
    }

    /// <summary>
    /// Get health metrics history for vehicle
    /// </summary>
    /// <param name="vehicleId">Vehicle ID</param>
    /// <param name="limit">Maximum number of records to return (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Historical health metrics ordered by date descending</returns>
    [HttpGet("{vehicleId:int}/history")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetHealthHistory(
        int vehicleId,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // SECURITY: Verify vehicle ownership for customer role
            if (!await VerifyVehicleOwnershipAsync(vehicleId, cancellationToken))
            {
                _logger.LogWarning("User {UserId} attempted to access health history for vehicle {VehicleId} without ownership",
                    GetCurrentUserId(), vehicleId);
                return Forbid();
            }

            var result = await _vehicleHealthService.GetHealthHistoryAsync(vehicleId, limit, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health history for Vehicle {VehicleId}", vehicleId);
            return StatusCode(500, new { success = false, message = "Error retrieving health history" });
        }
    }

    /// <summary>
    /// Get health metrics for specific work order
    /// </summary>
    /// <param name="workOrderId">Work order ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Health metrics recorded during this work order</returns>
    [HttpGet("work-order/{workOrderId:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHealthMetricsByWorkOrder(
        int workOrderId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _vehicleHealthService.GetHealthMetricsByWorkOrderAsync(workOrderId, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health metrics for WorkOrder {WorkOrderId}", workOrderId);
            return StatusCode(500, new { success = false, message = "Error retrieving health metrics" });
        }
    }

    #endregion

    #region Health Alerts

    /// <summary>
    /// Get vehicles needing health check soon
    /// </summary>
    /// <param name="serviceCenterId">Filter by service center (optional)</param>
    /// <param name="daysAhead">Number of days ahead to check (default: 30)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of vehicles with upcoming or overdue health checks</returns>
    /// <remarks>
    /// Returns vehicles where NextCheckDue is within the specified days ahead.
    /// Useful for proactive maintenance scheduling.
    /// </remarks>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehiclesNeedingHealthCheck(
        [FromQuery] int? serviceCenterId = null,
        [FromQuery] int daysAhead = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _vehicleHealthService.GetVehiclesNeedingHealthCheckAsync(
                serviceCenterId, daysAhead, cancellationToken);

            return Ok(new
            {
                success = true,
                data = result,
                summary = new
                {
                    totalVehicles = result.Count,
                    overdueCount = result.Count(v => v.DaysUntilDue < 0),
                    dueWithinWeek = result.Count(v => v.DaysUntilDue >= 0 && v.DaysUntilDue <= 7),
                    dueWithinMonth = result.Count(v => v.DaysUntilDue > 7 && v.DaysUntilDue <= 30)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting vehicles needing health check");
            return StatusCode(500, new { success = false, message = "Error retrieving health alerts" });
        }
    }

    /// <summary>
    /// Get critical health alerts (vehicles with poor condition)
    /// </summary>
    /// <param name="serviceCenterId">Filter by service center (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vehicles with overall condition below 40 (Fair or worse)</returns>
    [HttpGet("alerts/critical")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCriticalHealthAlerts(
        [FromQuery] int? serviceCenterId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all alerts and filter for critical condition
            var allAlerts = await _vehicleHealthService.GetVehiclesNeedingHealthCheckAsync(
                serviceCenterId, 365, cancellationToken); // Get all within a year

            var criticalAlerts = allAlerts
                .Where(v => v.LastOverallCondition.HasValue && v.LastOverallCondition.Value < 40)
                .OrderBy(v => v.LastOverallCondition)
                .ToList();

            return Ok(new
            {
                success = true,
                data = criticalAlerts,
                summary = new
                {
                    totalCritical = criticalAlerts.Count,
                    criticalCount = criticalAlerts.Count(v => v.HealthStatus == "Critical"),
                    poorCount = criticalAlerts.Count(v => v.HealthStatus == "Poor")
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting critical health alerts");
            return StatusCode(500, new { success = false, message = "Error retrieving critical alerts" });
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
    /// Verify that the current user owns the vehicle
    /// Staff/Admin/Manager/Technician bypass this check (via role)
    /// Customers must own the vehicle via CustomerId claim
    /// </summary>
    private async Task<bool> VerifyVehicleOwnershipAsync(int vehicleId, CancellationToken cancellationToken)
    {
        // Staff and above can access any vehicle
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

        // Get vehicle and verify CustomerId matches
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle {vehicleId} not found");
        }

        return vehicle.CustomerId == customerId;
    }

    #endregion
}
