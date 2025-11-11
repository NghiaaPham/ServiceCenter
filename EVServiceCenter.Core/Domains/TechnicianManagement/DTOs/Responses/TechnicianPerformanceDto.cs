namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;

/// <summary>
/// Detailed performance metrics for technician
/// Used for performance review and analytics
/// </summary>
public class TechnicianPerformanceDto
{
    public int TechnicianId { get; set; }
    public string TechnicianName { get; set; } = null!;
    public string? EmployeeCode { get; set; }

    /// <summary>
    /// Performance period
    /// </summary>
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Work order statistics
    /// </summary>
    public WorkOrderStatistics WorkOrders { get; set; } = new();

    /// <summary>
    /// Time management metrics
    /// </summary>
    public TimeManagementMetrics TimeMetrics { get; set; } = new();

    /// <summary>
    /// Quality metrics
    /// </summary>
    public QualityMetrics Quality { get; set; } = new();

    /// <summary>
    /// Customer satisfaction
    /// </summary>
    public CustomerSatisfactionMetrics CustomerSatisfaction { get; set; } = new();

    /// <summary>
    /// Skill utilization
    /// </summary>
    public List<SkillUtilizationDto> SkillUtilization { get; set; } = new();

    /// <summary>
    /// Overall performance score (0-100)
    /// Weighted average of all metrics
    /// </summary>
    public decimal OverallScore { get; set; }
}

public class WorkOrderStatistics
{
    /// <summary>
    /// Total work orders assigned
    /// </summary>
    public int TotalAssigned { get; set; }

    /// <summary>
    /// Work orders completed
    /// </summary>
    public int Completed { get; set; }

    /// <summary>
    /// Work orders in progress
    /// </summary>
    public int InProgress { get; set; }

    /// <summary>
    /// Work orders cancelled/failed
    /// </summary>
    public int Cancelled { get; set; }

    /// <summary>
    /// Completion rate (%)
    /// </summary>
    public decimal CompletionRate => TotalAssigned > 0 ? (Completed * 100.0m / TotalAssigned) : 0;

    /// <summary>
    /// Average work orders per day
    /// </summary>
    public decimal AveragePerDay { get; set; }
}

public class TimeManagementMetrics
{
    /// <summary>
    /// Average time to complete (hours)
    /// </summary>
    public decimal? AverageCompletionTimeHours { get; set; }

    /// <summary>
    /// Average estimated time (hours)
    /// </summary>
    public decimal? AverageEstimatedTimeHours { get; set; }

    /// <summary>
    /// Efficiency score (Actual / Estimated)
    /// > 1.0 = faster than estimate
    /// < 1.0 = slower than estimate
    /// </summary>
    public decimal? EfficiencyScore { get; set; }

    /// <summary>
    /// On-time completion rate (%)
    /// </summary>
    public decimal? OnTimeRate { get; set; }

    /// <summary>
    /// Total hours worked
    /// </summary>
    public decimal TotalHoursWorked { get; set; }

    /// <summary>
    /// Overtime hours
    /// </summary>
    public decimal? OvertimeHours { get; set; }
}

public class QualityMetrics
{
    /// <summary>
    /// Quality checks performed
    /// </summary>
    public int TotalQualityChecks { get; set; }

    /// <summary>
    /// Quality checks passed
    /// </summary>
    public int Passed { get; set; }

    /// <summary>
    /// Quality pass rate (%)
    /// </summary>
    public decimal PassRate => TotalQualityChecks > 0 ? (Passed * 100.0m / TotalQualityChecks) : 0;

    /// <summary>
    /// Average quality rating (1-5)
    /// </summary>
    public decimal? AverageQualityRating { get; set; }

    /// <summary>
    /// Rework rate (%)
    /// Work orders that needed rework
    /// </summary>
    public decimal? ReworkRate { get; set; }

    /// <summary>
    /// First-time fix rate (%)
    /// </summary>
    public decimal? FirstTimeFixRate { get; set; }
}

public class CustomerSatisfactionMetrics
{
    /// <summary>
    /// Total ratings received
    /// </summary>
    public int TotalRatings { get; set; }

    /// <summary>
    /// Average customer rating (1-5)
    /// </summary>
    public decimal? AverageRating { get; set; }

    /// <summary>
    /// 5-star ratings count
    /// </summary>
    public int FiveStarCount { get; set; }

    /// <summary>
    /// 1-2 star ratings count
    /// </summary>
    public int LowRatingCount { get; set; }

    /// <summary>
    /// Customer satisfaction score (%)
    /// Percentage of 4-5 star ratings
    /// </summary>
    public decimal? SatisfactionScore { get; set; }

    /// <summary>
    /// Number of customer complaints
    /// </summary>
    public int ComplaintCount { get; set; }

    /// <summary>
    /// Number of commendations
    /// </summary>
    public int CommendationCount { get; set; }
}

public class SkillUtilizationDto
{
    public string SkillName { get; set; } = null!;
    public string SkillLevel { get; set; } = null!;

    /// <summary>
    /// Times this skill was used
    /// </summary>
    public int UsageCount { get; set; }

    /// <summary>
    /// Percentage of total work orders
    /// </summary>
    public decimal UsagePercentage { get; set; }

    /// <summary>
    /// Success rate with this skill (%)
    /// </summary>
    public decimal? SuccessRate { get; set; }
}
