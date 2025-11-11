using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Technicians;

/// <summary>
/// Technician Query Operations
/// Domain: Technician Management - Read Operations
/// </summary>
[ApiController]
[Route("api/technicians")]
[ApiExplorerSettings(GroupName = "Technician Management")]
[Authorize]
public class TechnicianQueryController : ControllerBase
{
    private readonly ITechnicianService _technicianService;
    private readonly ILogger<TechnicianQueryController> _logger;

    public TechnicianQueryController(
        ITechnicianService technicianService,
        ILogger<TechnicianQueryController> logger)
    {
        _technicianService = technicianService;
        _logger = logger;
    }

    /// <summary>
    /// [List] Get all technicians with filtering and pagination
    /// </summary>
    /// <remarks>
    /// **Filters:**
    /// - ServiceCenterId: Filter by service center
    /// - Department: Filter by department
    /// - SkillName: Filter by skill (partial match)
    /// - IsActive: Active status
    /// - SearchTerm: Search by name, email, or employee code
    ///
    /// **Sorting:**
    /// - SortBy: name (default), hireDate, department, workload
    /// - SortDirection: asc (default), desc
    ///
    /// **Response includes:**
    /// - Current workload (active work orders count)
    /// - Availability status
    /// - Top 3 skills
    /// - Average customer rating
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTechnicians(
        [FromQuery] TechnicianQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _technicianService.GetTechniciansAsync(query, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting technicians");
            return StatusCode(500, new { success = false, message = "Error retrieving technicians" });
        }
    }

    /// <summary>
    /// [Details] Get technician by ID with full details
    /// </summary>
    /// <remarks>
    /// **Response includes:**
    /// - Basic info (name, email, phone, employee code, department)
    /// - Current workload and availability
    /// - All skills with certification details
    /// - Today's schedule
    /// - Performance summary (completion count, avg time, avg rating)
    /// </remarks>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTechnician(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _technicianService.GetTechnicianByIdAsync(id, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Technician {id} not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting technician {TechnicianId}", id);
            return StatusCode(500, new { success = false, message = "Error retrieving technician" });
        }
    }

    /// <summary>
    /// [Schedule] Get technician schedule for date range
    /// </summary>
    /// <remarks>
    /// Returns schedule entries including:
    /// - Work date and time (start, end, break)
    /// - Shift type
    /// - Available/booked minutes
    /// - Availability status
    ///
    /// **Limit:** Max 90 days range
    /// </remarks>
    [HttpGet("{id:int}/schedule")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSchedule(
        int id,
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var end = endDate ?? start.AddDays(7);

            var result = await _technicianService.GetScheduleAsync(id, start, end, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schedule for technician {TechnicianId}", id);
            return StatusCode(500, new { success = false, message = "Error retrieving schedule" });
        }
    }

    /// <summary>
    /// [Skills] Get technician skills with certifications
    /// </summary>
    /// <remarks>
    /// Returns all skills including:
    /// - Skill name and level
    /// - Certification details (date, expiry, certifying body)
    /// - Verification status
    ///
    /// **Note:** Expired certifications are flagged
    /// </remarks>
    [HttpGet("{id:int}/skills")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSkills(int id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _technicianService.GetSkillsAsync(id, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting skills for technician {TechnicianId}", id);
            return StatusCode(500, new { success = false, message = "Error retrieving skills" });
        }
    }

    /// <summary>
    /// [Performance] Get performance metrics for technician
    /// </summary>
    /// <remarks>
    /// Comprehensive performance report including:
    ///
    /// **Work Order Statistics:**
    /// - Total assigned, completed, in progress, cancelled
    /// - Completion rate
    /// - Average per day
    ///
    /// **Time Management:**
    /// - Average completion time
    /// - Efficiency score (actual vs estimated)
    /// - On-time completion rate
    ///
    /// **Quality:**
    /// - Quality check pass rate
    /// - Average quality rating
    /// - First-time fix rate
    ///
    /// **Customer Satisfaction:**
    /// - Average rating (1-5)
    /// - Rating distribution
    /// - Satisfaction score (% of 4-5 star ratings)
    ///
    /// **Overall Score:** Weighted average (0-100)
    ///
    /// **Default Period:** Last 30 days
    /// **Max Period:** 1 year
    /// </remarks>
    [HttpGet("{id:int}/performance")]
    [Authorize(Roles = "Admin,Manager,Supervisor")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPerformance(
        int id,
        [FromQuery] DateTime? periodStart,
        [FromQuery] DateTime? periodEnd,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _technicianService.GetPerformanceAsync(
                id, periodStart, periodEnd, cancellationToken);
            return Ok(new { success = true, data = result });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { success = false, message = $"Technician {id} not found" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance for technician {TechnicianId}", id);
            return StatusCode(500, new { success = false, message = "Error retrieving performance metrics" });
        }
    }
}
