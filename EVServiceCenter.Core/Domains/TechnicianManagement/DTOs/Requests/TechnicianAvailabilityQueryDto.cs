namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;

/// <summary>
/// Query for finding available technicians
/// Performance-optimized for auto-assignment
/// </summary>
public class TechnicianAvailabilityQueryDto
{
    /// <summary>
    /// Service center ID (required)
    /// </summary>
    public int ServiceCenterId { get; set; }

    /// <summary>
    /// Date to check availability
    /// </summary>
    public DateOnly WorkDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// Start time of work (optional, for specific time slot check)
    /// </summary>
    public TimeOnly? StartTime { get; set; }

    /// <summary>
    /// End time of work (optional)
    /// </summary>
    public TimeOnly? EndTime { get; set; }

    /// <summary>
    /// Required skills (comma-separated or list)
    /// Example: "Battery Replacement,Diagnostics"
    /// </summary>
    public List<string>? RequiredSkills { get; set; }

    /// <summary>
    /// Minimum skill level required
    /// </summary>
    public string? MinSkillLevel { get; set; } = "Intermediate";

    /// <summary>
    /// Estimated duration in minutes
    /// </summary>
    public int? EstimatedDurationMinutes { get; set; }

    /// <summary>
    /// Max current workload (number of active work orders)
    /// Default: 5
    /// </summary>
    public int MaxWorkload { get; set; } = 5;

    /// <summary>
    /// Include technicians on break
    /// </summary>
    public bool IncludeOnBreak { get; set; } = false;
}
