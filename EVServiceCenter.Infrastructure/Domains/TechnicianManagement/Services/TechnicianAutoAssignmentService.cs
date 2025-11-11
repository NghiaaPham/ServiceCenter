using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;
using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.TechnicianManagement.Services;

/// <summary>
/// Smart auto-assignment service for technicians
/// ALGORITHM:
/// 1. Filter by availability (schedule + workload)
/// 2. Filter by required skills
/// 3. Score each candidate (skills match + workload balance + performance)
/// 4. Return best match or top N candidates
/// </summary>
public class TechnicianAutoAssignmentService : ITechnicianAutoAssignmentService
{
    private readonly ITechnicianRepository _repository;
    private readonly ILogger<TechnicianAutoAssignmentService> _logger;

    // Scoring weights
    private const decimal SKILL_MATCH_WEIGHT = 0.40m;  // 40%
    private const decimal WORKLOAD_WEIGHT = 0.30m;     // 30%
    private const decimal PERFORMANCE_WEIGHT = 0.20m;  // 20%
    private const decimal AVAILABILITY_WEIGHT = 0.10m; // 10%

    public TechnicianAutoAssignmentService(
        ITechnicianRepository repository,
        ILogger<TechnicianAutoAssignmentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Find best technician for work order
    /// Returns single best match or null if none available
    /// </summary>
    public async Task<TechnicianSummaryDto?> FindBestTechnicianAsync(
        int serviceCenterId,
        List<string>? requiredSkills,
        int? estimatedDurationMinutes,
        DateOnly? scheduledDate = null,
        TimeOnly? scheduledTime = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Finding best technician for ServiceCenter {ServiceCenterId}, Skills: {Skills}",
            serviceCenterId, requiredSkills != null ? string.Join(", ", requiredSkills) : "None");

        var candidates = await FindTechnicianCandidatesAsync(
            serviceCenterId,
            requiredSkills,
            estimatedDurationMinutes,
            topN: 1,
            scheduledDate,
            scheduledTime,
            cancellationToken);

        var best = candidates.FirstOrDefault();

        if (best != null)
        {
            _logger.LogInformation(
                "Best match found: Technician {TechnicianId} ({Name}) with score {Score}",
                best.UserId, best.FullName, best.MatchScore);
        }
        else
        {
            _logger.LogWarning("No available technician found for criteria");
        }

        return best;
    }

    /// <summary>
    /// Find multiple technician candidates with ranking
    /// Returns top N candidates sorted by match score
    /// </summary>
    public async Task<List<TechnicianMatchDto>> FindTechnicianCandidatesAsync(
        int serviceCenterId,
        List<string>? requiredSkills,
        int? estimatedDurationMinutes,
        int topN = 5,
        DateOnly? scheduledDate = null,
        TimeOnly? scheduledTime = null,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Find available technicians
        var query = new TechnicianAvailabilityQueryDto
        {
            ServiceCenterId = serviceCenterId,
            WorkDate = scheduledDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            StartTime = scheduledTime,
            RequiredSkills = requiredSkills,
            EstimatedDurationMinutes = estimatedDurationMinutes,
            MaxWorkload = 5
        };

        var availableTechnicians = await _repository.FindAvailableTechniciansAsync(query, cancellationToken);

        if (!availableTechnicians.Any())
        {
            _logger.LogWarning("No available technicians found");
            return new List<TechnicianMatchDto>();
        }

        _logger.LogInformation("Found {Count} available technicians", availableTechnicians.Count);

        // Step 2: Get detailed skills for each technician
        var candidates = new List<TechnicianMatchDto>();

        foreach (var tech in availableTechnicians)
        {
            var skills = await _repository.GetSkillsAsync(tech.UserId, cancellationToken);

            var match = await ScoreTechnicianAsync(
                tech,
                skills,
                requiredSkills,
                cancellationToken);

            candidates.Add(match);
        }

        // Step 3: Sort by match score and return top N
        var topCandidates = candidates
            .OrderByDescending(c => c.MatchScore)
            .Take(topN)
            .ToList();

        _logger.LogInformation(
            "Top {TopN} candidates: {Candidates}",
            topN,
            string.Join(", ", topCandidates.Select(c => $"{c.FullName} ({c.MatchScore:F2})")));

        return topCandidates;
    }

    /// <summary>
    /// Balance workload across technicians
    /// Identifies overloaded technicians and suggests reassignment
    /// </summary>
    public async Task<WorkloadBalanceResult> BalanceWorkloadAsync(
        int serviceCenterId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Analyzing workload balance for ServiceCenter {ServiceCenterId}", serviceCenterId);

        // Get all technicians in service center
        var query = new TechnicianQueryDto
        {
            ServiceCenterId = serviceCenterId,
            IsActive = true,
            PageSize = 100
        };

        var technicians = await _repository.GetTechniciansAsync(query, cancellationToken);

        if (technicians.TotalCount == 0)
        {
            return new WorkloadBalanceResult
            {
                IsBalanced = true,
                TechniciansAnalyzed = 0,
                RebalanceActions = new List<string> { "No technicians found" }
            };
        }

        // Calculate statistics
        var workloads = technicians.Items.Select(t => t.CurrentWorkload).ToList();
        var avgWorkload = workloads.Average();
        var stdDev = CalculateStandardDeviation(workloads);

        // Check if balanced (std dev < 2)
        var isBalanced = stdDev < 2.0;

        var actions = new List<string>();

        if (!isBalanced)
        {
            // Identify overloaded and underloaded technicians
            var overloaded = technicians.Items
                .Where(t => t.CurrentWorkload > avgWorkload + 2)
                .OrderByDescending(t => t.CurrentWorkload)
                .ToList();

            var underloaded = technicians.Items
                .Where(t => t.CurrentWorkload < avgWorkload - 1)
                .OrderBy(t => t.CurrentWorkload)
                .ToList();

            foreach (var tech in overloaded)
            {
                actions.Add($"Technician {tech.FullName} is overloaded ({tech.CurrentWorkload} orders). " +
                    $"Consider reassigning {tech.CurrentWorkload - (int)avgWorkload} orders.");
            }

            foreach (var tech in underloaded.Take(3))
            {
                actions.Add($"Technician {tech.FullName} has low workload ({tech.CurrentWorkload} orders). " +
                    $"Available for {5 - tech.CurrentWorkload} more orders.");
            }
        }
        else
        {
            actions.Add($"Workload is well balanced. Average: {avgWorkload:F1}, Std Dev: {stdDev:F2}");
        }

        return new WorkloadBalanceResult
        {
            IsBalanced = isBalanced,
            TechniciansAnalyzed = technicians.TotalCount,
            WorkOrdersReassigned = 0, // Would be implemented in actual reassignment logic
            AverageWorkload = (decimal)avgWorkload,
            WorkloadStandardDeviation = (decimal)stdDev,
            RebalanceActions = actions
        };
    }

    #region Scoring Algorithm

    /// <summary>
    /// Score technician for work order match
    /// Combines multiple factors into single score (0-100)
    /// </summary>
    private async Task<TechnicianMatchDto> ScoreTechnicianAsync(
        TechnicianSummaryDto technician,
        List<TechnicianSkillDto> skills,
        List<string>? requiredSkills,
        CancellationToken cancellationToken)
    {
        // 1. Skills match score (0-100)
        var (skillScore, matchedSkills, missingSkills) = CalculateSkillsMatchScore(skills, requiredSkills);

        // 2. Workload score (0-100, lower workload = higher score)
        var workloadScore = CalculateWorkloadScore(technician.CurrentWorkload);

        // 3. Performance score (0-100)
        var performanceScore = CalculatePerformanceScore(technician.AverageRating);

        // 4. Availability score (0-100)
        var availabilityScore = technician.IsAvailable ? 100m : 0m;

        // Calculate weighted total
        var totalScore =
            (skillScore * SKILL_MATCH_WEIGHT) +
            (workloadScore * WORKLOAD_WEIGHT) +
            (performanceScore * PERFORMANCE_WEIGHT) +
            (availabilityScore * AVAILABILITY_WEIGHT);

        // Generate recommendation reason
        var reason = GenerateRecommendationReason(
            skillScore, workloadScore, performanceScore, matchedSkills, missingSkills);

        return new TechnicianMatchDto
        {
            UserId = technician.UserId,
            FullName = technician.FullName,
            Email = technician.Email,
            PhoneNumber = technician.PhoneNumber,
            EmployeeCode = technician.EmployeeCode,
            Department = technician.Department,
            CurrentWorkload = technician.CurrentWorkload,
            IsAvailable = technician.IsAvailable,
            TopSkills = technician.TopSkills,
            AverageRating = technician.AverageRating,
            IsActive = technician.IsActive,
            MatchScore = Math.Round(totalScore, 2),
            SkillsMatchPercentage = Math.Round(skillScore, 2),
            WorkloadScore = Math.Round(workloadScore, 2),
            PerformanceScore = Math.Round(performanceScore, 2),
            AvailabilityScore = Math.Round(availabilityScore, 2),
            MatchedSkills = matchedSkills,
            MissingSkills = missingSkills,
            RecommendationReason = reason
        };
    }

    /// <summary>
    /// Calculate skills match score
    /// Perfect match = 100, partial match = proportional, no match = 0
    /// </summary>
    private (decimal score, List<string> matched, List<string> missing) CalculateSkillsMatchScore(
        List<TechnicianSkillDto> technicianSkills,
        List<string>? requiredSkills)
    {
        if (requiredSkills == null || !requiredSkills.Any())
            return (100m, new List<string>(), new List<string>());

        var verifiedSkills = technicianSkills
            .Where(s => s.IsVerified && !s.IsExpired)
            .Select(s => s.SkillName.ToLower())
            .ToHashSet();

        var matched = new List<string>();
        var missing = new List<string>();

        foreach (var required in requiredSkills)
        {
            var requiredLower = required.ToLower();
            if (verifiedSkills.Any(s => s.Contains(requiredLower) || requiredLower.Contains(s)))
                matched.Add(required);
            else
                missing.Add(required);
        }

        var matchPercentage = (decimal)matched.Count * 100 / requiredSkills.Count;
        return (matchPercentage, matched, missing);
    }

    /// <summary>
    /// Calculate workload score
    /// 0 orders = 100, 5 orders = 0, linear scale
    /// </summary>
    private decimal CalculateWorkloadScore(int currentWorkload)
    {
        const int maxWorkload = 5;
        return Math.Max(0, (maxWorkload - currentWorkload) * 100.0m / maxWorkload);
    }

    /// <summary>
    /// Calculate performance score from average rating
    /// 5.0 rating = 100, 1.0 rating = 0, linear scale
    /// </summary>
    private decimal CalculatePerformanceScore(decimal? averageRating)
    {
        if (!averageRating.HasValue)
            return 70m; // Default score for no ratings

        return ((averageRating.Value - 1) / 4) * 100;
    }

    /// <summary>
    /// Generate human-readable recommendation reason
    /// </summary>
    private string GenerateRecommendationReason(
        decimal skillScore,
        decimal workloadScore,
        decimal performanceScore,
        List<string> matchedSkills,
        List<string> missingSkills)
    {
        var reasons = new List<string>();

        if (skillScore == 100)
            reasons.Add("Perfect skills match");
        else if (skillScore >= 75)
            reasons.Add($"Good skills match ({matchedSkills.Count}/{matchedSkills.Count + missingSkills.Count})");
        else if (skillScore >= 50)
            reasons.Add($"Partial skills match ({matchedSkills.Count}/{matchedSkills.Count + missingSkills.Count})");

        if (workloadScore >= 80)
            reasons.Add("Low workload");
        else if (workloadScore <= 40)
            reasons.Add("High workload");

        if (performanceScore >= 80)
            reasons.Add("Excellent performance history");

        if (missingSkills.Any())
            reasons.Add($"Missing: {string.Join(", ", missingSkills)}");

        return string.Join("; ", reasons);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Calculate standard deviation for workload balance
    /// </summary>
    private double CalculateStandardDeviation(List<int> values)
    {
        if (values.Count == 0) return 0;

        var avg = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }

    #endregion
}
