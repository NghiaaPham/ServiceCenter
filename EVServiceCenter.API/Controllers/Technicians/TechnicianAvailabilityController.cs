using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Technicians;

/// <summary>
/// Technician Availability & Auto-Assignment
/// Domain: Technician Management - Availability Operations
/// </summary>
[ApiController]
[Route("api/technicians")]
[ApiExplorerSettings(GroupName = "Technician Availability")]
[Authorize]
public class TechnicianAvailabilityController : ControllerBase
{
    private readonly ITechnicianService _technicianService;
    private readonly ITechnicianAutoAssignmentService _autoAssignmentService;
    private readonly ILogger<TechnicianAvailabilityController> _logger;

    public TechnicianAvailabilityController(
        ITechnicianService technicianService,
        ITechnicianAutoAssignmentService autoAssignmentService,
        ILogger<TechnicianAvailabilityController> logger)
    {
        _technicianService = technicianService;
        _autoAssignmentService = autoAssignmentService;
        _logger = logger;
    }

    /// <summary>
    /// [Find Available] Find available technicians for work order
    /// </summary>
    /// <remarks>
    /// Finds technicians who are:
    /// - Active and assigned to specified service center
    /// - Available on specified date (has schedule)
    /// - Not overloaded (current workload < max capacity)
    /// - Have required skills (if specified)
    ///
    /// **Query Parameters:**
    /// - ServiceCenterId: Required
    /// - WorkDate: Date to check (default: today)
    /// - RequiredSkills: List of skills needed
    /// - MaxWorkload: Maximum current workload (default: 5)
    ///
    /// **Response:**
    /// List of available technicians sorted by:
    /// 1. Workload (ascending)
    /// 2. Average rating (descending)
    ///
    /// **Use Case:** Manual assignment by staff
    /// </remarks>
    [HttpGet("available")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> FindAvailableTechnicians(
        [FromQuery] TechnicianAvailabilityQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _technicianService.FindAvailableTechniciansAsync(query, cancellationToken);
            return Ok(new
            {
                success = true,
                data = result,
                count = result.Count,
                message = result.Any() ? $"Found {result.Count} available technicians" : "No available technicians found"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding available technicians");
            return StatusCode(500, new { success = false, message = "Error finding available technicians" });
        }
    }

    /// <summary>
    /// [Auto-Assign] Find best technician using smart algorithm
    /// </summary>
    /// <remarks>
    /// **Smart Assignment Algorithm:**
    ///
    /// **Step 1: Filter**
    /// - Service center match
    /// - Date availability (schedule + workload)
    /// - Required skills match
    ///
    /// **Step 2: Score (0-100)**
    /// Each technician scored on:
    /// - Skills match: 40% weight
    /// - Workload balance: 30% weight
    /// - Performance (avg rating): 20% weight
    /// - Availability: 10% weight
    ///
    /// **Step 3: Return Best Match**
    /// Returns single technician with highest score.
    ///
    /// **Query Parameters:**
    /// - ServiceCenterId: Required
    /// - RequiredSkills: Skills needed for work order
    /// - EstimatedDurationMinutes: Estimated work time
    /// - ScheduledDate: When work is scheduled
    ///
    /// **Response:**
    /// Best matching technician or null if none available.
    ///
    /// **Use Case:** Automatic assignment by system
    /// </remarks>
    [HttpPost("auto-assign/best")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FindBestTechnician(
        [FromBody] TechnicianAvailabilityQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _autoAssignmentService.FindBestTechnicianAsync(
                query.ServiceCenterId,
                query.RequiredSkills,
                query.EstimatedDurationMinutes,
                query.WorkDate,
                query.StartTime,
                cancellationToken);

            if (result == null)
                return NotFound(new
                {
                    success = false,
                    message = "No technician available for the specified criteria"
                });

            return Ok(new
            {
                success = true,
                data = result,
                message = $"Best match: {result.FullName}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding best technician");
            return StatusCode(500, new { success = false, message = "Error in auto-assignment" });
        }
    }

    /// <summary>
    /// [Auto-Assign] Get top N technician candidates with scores
    /// </summary>
    /// <remarks>
    /// Returns multiple candidates ranked by match score.
    /// Useful for showing options to staff for final decision.
    ///
    /// **Response includes:**
    /// - Match score (0-100)
    /// - Skills match percentage
    /// - Workload score
    /// - Performance score
    /// - Matched/missing skills
    /// - Recommendation reason
    ///
    /// **Default:** Top 5 candidates
    /// </remarks>
    [HttpPost("auto-assign/candidates")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> FindTechnicianCandidates(
        [FromBody] TechnicianAvailabilityQueryDto query,
        CancellationToken cancellationToken,
        [FromQuery] int topN = 5)
    {
        try
        {
            var result = await _autoAssignmentService.FindTechnicianCandidatesAsync(
                query.ServiceCenterId,
                query.RequiredSkills,
                query.EstimatedDurationMinutes,
                topN,
                query.WorkDate,
                query.StartTime,
                cancellationToken);

            return Ok(new
            {
                success = true,
                data = result,
                count = result.Count,
                message = result.Any()
                    ? $"Found {result.Count} candidates"
                    : "No candidates available"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding technician candidates");
            return StatusCode(500, new { success = false, message = "Error finding candidates" });
        }
    }

    /// <summary>
    /// [Workload] Analyze workload balance for service center
    /// </summary>
    /// <remarks>
    /// Analyzes workload distribution across technicians:
    ///
    /// **Calculates:**
    /// - Average workload
    /// - Standard deviation
    /// - Overloaded technicians (> avg + 2)
    /// - Underloaded technicians (< avg - 1)
    ///
    /// **Response:**
    /// - IsBalanced: true if std dev < 2
    /// - Recommendations for rebalancing
    ///
    /// **Use Case:** Manager dashboard, workload optimization
    /// </remarks>
    [HttpGet("workload-balance/{serviceCenterId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> AnalyzeWorkloadBalance(
        int serviceCenterId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _autoAssignmentService.BalanceWorkloadAsync(serviceCenterId, cancellationToken);

            return Ok(new
            {
                success = true,
                data = result,
                message = result.IsBalanced
                    ? "Workload is well balanced"
                    : "Workload imbalance detected. See recommendations."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing workload balance for service center {ServiceCenterId}",
                serviceCenterId);
            return StatusCode(500, new { success = false, message = "Error analyzing workload" });
        }
    }
}
