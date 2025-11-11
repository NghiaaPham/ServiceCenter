using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;

/// <summary>
/// Request to add skill to technician
/// </summary>
public class AddTechnicianSkillRequestDto
{
    /// <summary>
    /// Skill name (e.g., "Battery Replacement", "Diagnostics")
    /// </summary>
    [Required]
    [StringLength(100)]
    public string SkillName { get; set; } = null!;

    /// <summary>
    /// Skill level: Beginner, Intermediate, Expert
    /// </summary>
    [Required]
    [StringLength(20)]
    public string SkillLevel { get; set; } = null!;

    /// <summary>
    /// Certification date (if applicable)
    /// </summary>
    public DateOnly? CertificationDate { get; set; }

    /// <summary>
    /// Certification expiry date
    /// </summary>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>
    /// Certifying organization
    /// </summary>
    [StringLength(100)]
    public string? CertifyingBody { get; set; }

    /// <summary>
    /// Certification number/ID
    /// </summary>
    [StringLength(50)]
    public string? CertificationNumber { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
}
