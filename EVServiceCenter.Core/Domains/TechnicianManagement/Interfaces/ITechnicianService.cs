using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;

/// <summary>
/// Service for technician management business logic
/// </summary>
public interface ITechnicianService
{
    /// <summary>
    /// Get technicians with filtering and pagination
    /// </summary>
    Task<PagedResult<TechnicianSummaryDto>> GetTechniciansAsync(
        TechnicianQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get technician details by ID
    /// </summary>
    Task<TechnicianResponseDto> GetTechnicianByIdAsync(
        int technicianId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find available technicians based on criteria
    /// </summary>
    Task<List<TechnicianSummaryDto>> FindAvailableTechniciansAsync(
        TechnicianAvailabilityQueryDto query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get technician schedule for date range
    /// </summary>
    Task<List<TechnicianScheduleSummaryDto>> GetScheduleAsync(
        int technicianId,
        DateOnly startDate,
        DateOnly endDate,
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
    /// Verify technician skill
    /// </summary>
    Task<bool> VerifySkillAsync(
        int technicianId,
        int skillId,
        int verifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance metrics for technician
    /// </summary>
    Task<TechnicianPerformanceDto> GetPerformanceAsync(
        int technicianId,
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        CancellationToken cancellationToken = default);

    // ? NEW: Technician Self-Service Methods
    
    /// <summary>
    /// Get technician's own schedule (for self-service portal)
    /// </summary>
    Task<List<TechnicianScheduleResponseDto>> GetMyScheduleAsync(
        int technicianId,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get technician's own work orders (for self-service portal)
    /// </summary>
    Task<List<TechnicianWorkOrderResponseDto>> GetMyWorkOrdersAsync(
        int technicianId,
        int? statusId = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get technician's own performance metrics (for self-service portal)
    /// </summary>
    Task<TechnicianPerformanceDto> GetMyPerformanceAsync(
        int technicianId,
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get technician's own ratings from customers (for self-service portal)
    /// </summary>
    Task<List<TechnicianRatingResponseDto>> GetMyRatingsAsync(
        int technicianId,
        int? minRating = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Request time off (vacation, sick leave, etc.)
    /// </summary>
    Task<bool> RequestTimeOffAsync(
        RequestTimeOffDto request,
        CancellationToken cancellationToken = default);
}
