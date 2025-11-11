using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;

/// <summary>
/// Repository for technician data access
/// Performance-optimized queries with caching strategy
/// </summary>
public interface ITechnicianRepository
{
    /// <summary>
    /// Get paginated list of technicians with filters
    /// PERFORMANCE: Single query with necessary includes
    /// </summary>
    Task<PagedResult<TechnicianSummaryDto>> GetTechniciansAsync(
        TechnicianQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get technician by ID with full details
    /// PERFORMANCE: Includes skills, schedule, performance summary
    /// </summary>
    Task<TechnicianResponseDto?> GetTechnicianByIdAsync(
        int technicianId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find available technicians for auto-assignment
    /// PERFORMANCE: Optimized query with workload calculation
    /// </summary>
    Task<List<TechnicianSummaryDto>> FindAvailableTechniciansAsync(
        TechnicianAvailabilityQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get technician's schedule for date range
    /// </summary>
    Task<List<TechnicianScheduleSummaryDto>> GetScheduleAsync(
        int technicianId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get technician's schedule for specific date and service center
    /// Used for shift validation
    /// </summary>
    Task<TechnicianSchedule?> GetScheduleByDateAsync(
        int technicianId,
        int serviceCenterId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current workload (active work orders count)
    /// PERFORMANCE: Cached with 5-minute expiration
    /// </summary>
    Task<int> GetCurrentWorkloadAsync(
        int technicianId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if technician is available for time slot
    /// PERFORMANCE: Quick boolean check without full data load
    /// </summary>
    Task<bool> IsAvailableAsync(
        int technicianId,
        DateOnly date,
        TimeOnly? startTime = null,
        TimeOnly? endTime = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get technician skills
    /// </summary>
    Task<List<TechnicianSkillDto>> GetSkillsAsync(
        int technicianId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add skill to technician
    /// </summary>
    Task<TechnicianSkillDto> AddSkillAsync(
        int technicianId,
        AddTechnicianSkillRequestDto request,
        int addedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove skill from technician
    /// </summary>
    Task<bool> RemoveSkillAsync(
        int technicianId,
        int skillId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify technician skill (by manager/supervisor)
    /// </summary>
    Task<bool> VerifySkillAsync(
        int technicianId,
        int skillId,
        int verifiedBy,
        CancellationToken cancellationToken = default);

    // ✅ NEW: Technician Self-Service Repository Methods

    /// <summary>
    /// Get technician's own schedule (self-service)
    /// </summary>
    Task<List<TechnicianScheduleResponseDto>> GetMyScheduleAsync(
        int technicianId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get technician's own work orders (self-service)
    /// </summary>
    Task<List<TechnicianWorkOrderResponseDto>> GetMyWorkOrdersAsync(
        int technicianId,
        int? statusId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get technician's own ratings (self-service)
    /// </summary>
    Task<List<TechnicianRatingResponseDto>> GetMyRatingsAsync(
        int technicianId,
        int? minRating,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Request time off
    /// </summary>
    Task<bool> RequestTimeOffAsync(
        RequestTimeOffDto request,
        CancellationToken cancellationToken = default);
}
