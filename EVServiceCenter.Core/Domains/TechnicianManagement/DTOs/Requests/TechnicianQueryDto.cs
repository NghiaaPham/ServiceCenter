namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;

/// <summary>
/// Query parameters for filtering and paginating technicians
/// </summary>
public class TechnicianQueryDto
{
    /// <summary>
    /// Service center ID to filter technicians
    /// </summary>
    public int? ServiceCenterId { get; set; }

    /// <summary>
    /// Department to filter
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Skill name to match (for skill-based filtering)
    /// </summary>
    public string? SkillName { get; set; }

    /// <summary>
    /// Minimum skill level (Beginner, Intermediate, Expert)
    /// </summary>
    public string? MinSkillLevel { get; set; }

    /// <summary>
    /// Only show active technicians
    /// </summary>
    public bool? IsActive { get; set; } = true;

    /// <summary>
    /// Search by name, email, or employee code
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Sort field (name, hireDate, workload)
    /// </summary>
    public string SortBy { get; set; } = "name";

    /// <summary>
    /// Sort direction (asc, desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; } = 20;
}
