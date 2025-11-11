using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EVServiceCenter.API.Controllers.Technicians;

/// <summary>
/// Technician Skill Management
/// Domain: Technician Management - Skill Operations
/// </summary>
[ApiController]
[Route("api/technicians")]
[ApiExplorerSettings(GroupName = "Technician Skills")]
[Authorize]
public class TechnicianSkillController : ControllerBase
{
    private readonly ITechnicianService _technicianService;
    private readonly ILogger<TechnicianSkillController> _logger;

    public TechnicianSkillController(
        ITechnicianService technicianService,
        ILogger<TechnicianSkillController> logger)
    {
        _technicianService = technicianService;
        _logger = logger;
    }

    /// <summary>
    /// [Add Skill] Add new skill to technician
    /// </summary>
    /// <remarks>
    /// **Required fields:**
    /// - SkillName: e.g., "Battery Replacement", "Diagnostics"
    /// - SkillLevel: Beginner | Intermediate | Expert
    ///
    /// **Optional fields:**
    /// - CertificationDate: When certified
    /// - ExpiryDate: Certification expiry
    /// - CertifyingBody: Organization that certified
    /// - CertificationNumber: Certificate ID/number
    /// - Notes: Additional information
    ///
    /// **Business Rules:**
    /// - Skill name must be unique for technician
    /// - Cannot add expired certifications
    /// - Skill requires verification by manager/supervisor
    ///
    /// **Authorization:** Staff, Manager, or Admin
    /// </remarks>
    [HttpPost("{id:int}/skills")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddSkill(
        int id,
        [FromBody] AddTechnicianSkillRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _technicianService.AddSkillAsync(id, request, userId, cancellationToken);

            return CreatedAtAction(
                nameof(TechnicianQueryController.GetSkills),
                "TechnicianQuery",
                new { id },
                new { success = true, data = result, message = "Skill added successfully. Pending verification." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding skill to technician {TechnicianId}", id);
            return StatusCode(500, new { success = false, message = "Error adding skill" });
        }
    }

    /// <summary>
    /// [Remove Skill] Remove skill from technician
    /// </summary>
    /// <remarks>
    /// Permanently removes skill and certification record.
    ///
    /// **Authorization:** Manager or Admin only
    /// </remarks>
    [HttpDelete("{technicianId:int}/skills/{skillId:int}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSkill(
        int technicianId,
        int skillId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _technicianService.RemoveSkillAsync(technicianId, skillId, cancellationToken);

            if (result)
                return NoContent();

            return NotFound(new { success = false, message = "Skill not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing skill {SkillId} from technician {TechnicianId}",
                skillId, technicianId);
            return StatusCode(500, new { success = false, message = "Error removing skill" });
        }
    }

    /// <summary>
    /// [Verify Skill] Verify technician skill
    /// </summary>
    /// <remarks>
    /// Manager/Supervisor verifies that technician actually has this skill.
    ///
    /// **Effect:**
    /// - Sets IsVerified = true
    /// - Records verifier ID and date
    /// - Skill becomes visible in auto-assignment matching
    ///
    /// **Authorization:** Manager or Supervisor only
    /// </remarks>
    [HttpPost("{technicianId:int}/skills/{skillId:int}/verify")]
    [Authorize(Roles = "Admin,Manager,Supervisor")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifySkill(
        int technicianId,
        int skillId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _technicianService.VerifySkillAsync(technicianId, skillId, userId, cancellationToken);

            if (result)
                return Ok(new { success = true, message = "Skill verified successfully" });

            return NotFound(new { success = false, message = "Skill not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying skill {SkillId} for technician {TechnicianId}",
                skillId, technicianId);
            return StatusCode(500, new { success = false, message = "Error verifying skill" });
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
