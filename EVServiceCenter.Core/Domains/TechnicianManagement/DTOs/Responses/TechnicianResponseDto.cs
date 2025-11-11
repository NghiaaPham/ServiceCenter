namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;

/// <summary>
/// Technician details with workload and availability info
/// Optimized for list views and dashboard
/// </summary>
public class TechnicianResponseDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmployeeCode { get; set; }
    public string? Department { get; set; }
    public DateOnly? HireDate { get; set; }
    public bool IsActive { get; set; }
    public string? ProfilePicture { get; set; }

    /// <summary>
    /// Current workload (number of active work orders)
    /// </summary>
    public int CurrentWorkload { get; set; }

    /// <summary>
    /// Maximum capacity (from settings, default: 5)
    /// </summary>
    public int MaxCapacity { get; set; } = 5;

    /// <summary>
    /// Availability percentage (0-100)
    /// Formula: ((MaxCapacity - CurrentWorkload) / MaxCapacity) * 100
    /// </summary>
    public decimal AvailabilityPercentage { get; set; }

    /// <summary>
    /// Is currently available for new work
    /// </summary>
    public bool IsAvailable { get; set; }

    /// <summary>
    /// List of skills with levels
    /// </summary>
    public List<TechnicianSkillDto> Skills { get; set; } = new();

    /// <summary>
    /// Today's schedule (if exists)
    /// </summary>
    public TechnicianScheduleSummaryDto? TodaySchedule { get; set; }

    /// <summary>
    /// Performance summary
    /// </summary>
    public TechnicianPerformanceSummaryDto? Performance { get; set; }
}

/// <summary>
/// Technician skill details
/// </summary>
public class TechnicianSkillDto
{
    public int SkillId { get; set; }
    public string SkillName { get; set; } = null!;
    public string? SkillLevel { get; set; }
    public DateOnly? CertificationDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsVerified { get; set; }
    public string? CertifyingBody { get; set; }
}
