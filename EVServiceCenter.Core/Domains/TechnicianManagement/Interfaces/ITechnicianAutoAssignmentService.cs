using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;

/// <summary>
/// Service for auto-assigning technicians to work orders
/// Uses smart algorithm based on skills, workload, and availability
/// </summary>
public interface ITechnicianAutoAssignmentService
{
    /// <summary>
    /// Find best technician for work order
    /// ALGORITHM:
    /// 1. Filter by skills match (required skills)
    /// 2. Filter by availability (schedule + workload)
    /// 3. Sort by score (skills match + workload balance + performance)
    /// 4. Return top candidate
    /// </summary>
    /// <param name="serviceCenterId">Service center ID</param>
    /// <param name="requiredSkills">Required skills for work order</param>
    /// <param name="estimatedDurationMinutes">Estimated duration</param>
    /// <param name="scheduledDate">Scheduled date (optional)</param>
    /// <param name="scheduledTime">Scheduled time (optional)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Best matching technician or null if none available</returns>
    Task<TechnicianSummaryDto?> FindBestTechnicianAsync(
        int serviceCenterId,
        List<string>? requiredSkills,
        int? estimatedDurationMinutes,
        DateOnly? scheduledDate = null,
        TimeOnly? scheduledTime = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find multiple technician candidates with ranking
    /// Returns top N candidates sorted by match score
    /// </summary>
    Task<List<TechnicianMatchDto>> FindTechnicianCandidatesAsync(
        int serviceCenterId,
        List<string>? requiredSkills,
        int? estimatedDurationMinutes,
        int topN = 5,
        DateOnly? scheduledDate = null,
        TimeOnly? scheduledTime = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Balance workload across technicians
    /// Redistributes work orders when imbalance detected
    /// </summary>
    Task<WorkloadBalanceResult> BalanceWorkloadAsync(
        int serviceCenterId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Technician match with score
/// </summary>
public class TechnicianMatchDto : TechnicianSummaryDto
{
    /// <summary>
    /// Match score (0-100)
    /// Higher = better match
    /// </summary>
    public decimal MatchScore { get; set; }

    /// <summary>
    /// Skills match percentage
    /// </summary>
    public decimal SkillsMatchPercentage { get; set; }

    /// <summary>
    /// Workload score (lower workload = higher score)
    /// </summary>
    public decimal WorkloadScore { get; set; }

    /// <summary>
    /// Performance score
    /// </summary>
    public decimal PerformanceScore { get; set; }

    /// <summary>
    /// Availability score
    /// </summary>
    public decimal AvailabilityScore { get; set; }

    /// <summary>
    /// Matched skills (for display)
    /// </summary>
    public List<string> MatchedSkills { get; set; } = new();

    /// <summary>
    /// Missing skills (if any)
    /// </summary>
    public List<string> MissingSkills { get; set; } = new();

    /// <summary>
    /// Recommendation reason
    /// </summary>
    public string? RecommendationReason { get; set; }
}

/// <summary>
/// Workload balance result
/// </summary>
public class WorkloadBalanceResult
{
    public bool IsBalanced { get; set; }
    public int TechniciansAnalyzed { get; set; }
    public int WorkOrdersReassigned { get; set; }
    public decimal AverageWorkload { get; set; }
    public decimal WorkloadStandardDeviation { get; set; }
    public List<string> RebalanceActions { get; set; } = new();
}
