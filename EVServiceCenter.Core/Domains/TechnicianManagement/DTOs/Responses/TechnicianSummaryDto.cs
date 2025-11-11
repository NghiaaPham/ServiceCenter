namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;

/// <summary>
/// Lightweight technician summary for list views
/// Performance-optimized with minimal data
/// </summary>
public class TechnicianSummaryDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }

    /// <summary>
    /// Current workload count
    /// </summary>
    public int CurrentWorkload { get; set; }

    /// <summary>
    /// Is available for new work
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// Top 3 skills (comma-separated)
    /// </summary>
    public string? TopSkills { get; set; }

    /// <summary>
    /// Average rating
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// Profile status
    /// </summary>
    public bool IsActive { get; set; }
}
