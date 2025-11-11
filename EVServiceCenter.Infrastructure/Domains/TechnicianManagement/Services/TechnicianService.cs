using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;
using EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces;
using EVServiceCenter.Infrastructure.Domains.TechnicianManagement.Repositories;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.TechnicianManagement.Services;

/// <summary>
/// Service for technician management business logic
/// Implements ITechnicianService with validation and business rules
/// </summary>
public class TechnicianService : ITechnicianService
{
    private readonly ITechnicianRepository _repository;
    private readonly TechnicianPerformanceRepository _performanceRepository;
    private readonly ILogger<TechnicianService> _logger;

    public TechnicianService(
        ITechnicianRepository repository,
        TechnicianPerformanceRepository performanceRepository,
        ILogger<TechnicianService> logger)
    {
        _repository = repository;
        _performanceRepository = performanceRepository;
        _logger = logger;
    }

    #region Query Operations

    public async Task<PagedResult<TechnicianSummaryDto>> GetTechniciansAsync(
        TechnicianQueryDto query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting technicians with filters: {@Query}", query);
        return await _repository.GetTechniciansAsync(query, cancellationToken);
    }

    public async Task<TechnicianResponseDto> GetTechnicianByIdAsync(
        int technicianId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting technician {TechnicianId}", technicianId);

        var technician = await _repository.GetTechnicianByIdAsync(technicianId, cancellationToken);

        if (technician == null)
            throw new KeyNotFoundException($"Technician {technicianId} not found");

        return technician;
    }

    public async Task<List<TechnicianSummaryDto>> FindAvailableTechniciansAsync(
        TechnicianAvailabilityQueryDto query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Finding available technicians for ServiceCenter {ServiceCenterId} on {Date}",
            query.ServiceCenterId, query.WorkDate);

        // Validate required fields
        if (query.ServiceCenterId <= 0)
            throw new ArgumentException("ServiceCenterId is required", nameof(query.ServiceCenterId));

        return await _repository.FindAvailableTechniciansAsync(query, cancellationToken);
    }

    public async Task<List<TechnicianScheduleSummaryDto>> GetScheduleAsync(
        int technicianId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting schedule for technician {TechnicianId} from {StartDate} to {EndDate}",
            technicianId, startDate, endDate);

        // Validate date range
        if (endDate < startDate)
            throw new ArgumentException("End date must be after start date");

        if ((endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days > 90)
            throw new ArgumentException("Date range cannot exceed 90 days");

        return await _repository.GetScheduleAsync(technicianId, startDate, endDate, cancellationToken);
    }

    #endregion

    #region Skill Management

    public async Task<List<TechnicianSkillDto>> GetSkillsAsync(
        int technicianId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting skills for technician {TechnicianId}", technicianId);
        return await _repository.GetSkillsAsync(technicianId, cancellationToken);
    }

    public async Task<TechnicianSkillDto> AddSkillAsync(
        int technicianId,
        AddTechnicianSkillRequestDto request,
        int addedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Adding skill '{SkillName}' to technician {TechnicianId}",
            request.SkillName, technicianId);

        // Business rule: Check for duplicate skill
        var existingSkills = await _repository.GetSkillsAsync(technicianId, cancellationToken);
        if (existingSkills.Any(s =>
            s.SkillName.Equals(request.SkillName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"Skill '{request.SkillName}' already exists for this technician");
        }

        // Business rule: Validate skill level
        var validLevels = new[] { "Beginner", "Intermediate", "Expert" };
        if (!validLevels.Contains(request.SkillLevel, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"Skill level must be one of: {string.Join(", ", validLevels)}",
                nameof(request.SkillLevel));
        }

        // Business rule: Certification validation
        if (request.ExpiryDate.HasValue && request.ExpiryDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new ArgumentException("Cannot add skill with expired certification");
        }

        return await _repository.AddSkillAsync(technicianId, request, addedBy, cancellationToken);
    }

    public async Task<bool> RemoveSkillAsync(
        int technicianId,
        int skillId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Removing skill {SkillId} from technician {TechnicianId}",
            skillId, technicianId);

        return await _repository.RemoveSkillAsync(technicianId, skillId, cancellationToken);
    }

    public async Task<bool> VerifySkillAsync(
        int technicianId,
        int skillId,
        int verifiedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Verifying skill {SkillId} for technician {TechnicianId} by user {VerifiedBy}",
            skillId, technicianId, verifiedBy);

        // Business rule: Only managers/supervisors can verify
        // (Authorization should be handled at controller level)

        return await _repository.VerifySkillAsync(technicianId, skillId, verifiedBy, cancellationToken);
    }

    #endregion

    #region Performance

    public async Task<TechnicianPerformanceDto> GetPerformanceAsync(
        int technicianId,
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        // Default period: Last 30 days
        var start = periodStart ?? DateTime.UtcNow.AddDays(-30);
        var end = periodEnd ?? DateTime.UtcNow;

        _logger.LogInformation(
            "Getting performance metrics for technician {TechnicianId} from {Start} to {End}",
            technicianId, start, end);

        // Validate date range
        if (end < start)
            throw new ArgumentException("End date must be after start date");

        if ((end - start).Days > 365)
            throw new ArgumentException("Performance period cannot exceed 1 year");

        return await _performanceRepository.GetPerformanceMetricsAsync(
            technicianId, start, end, cancellationToken);
    }

    #endregion

    #region Technician Self-Service Methods

    public async Task<List<TechnicianScheduleResponseDto>> GetMyScheduleAsync(
        int technicianId,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Default: Next 7 days from today
        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var end = endDate ?? start.AddDays(7);

        _logger.LogInformation(
            "Technician {TechnicianId} viewing own schedule from {StartDate} to {EndDate}",
            technicianId, start, end);

        // Validate date range
        if (end < start)
            throw new ArgumentException("End date must be after start date");

        if ((end.ToDateTime(TimeOnly.MinValue) - start.ToDateTime(TimeOnly.MinValue)).Days > 30)
            throw new ArgumentException("Date range cannot exceed 30 days for self-service");

        return await _repository.GetMyScheduleAsync(technicianId, start, end, cancellationToken);
    }

    public async Task<List<TechnicianWorkOrderResponseDto>> GetMyWorkOrdersAsync(
        int technicianId,
        int? statusId = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Default: Last 30 days to next 7 days
        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        _logger.LogInformation(
            "Technician {TechnicianId} viewing own work orders (Status: {StatusId}, {StartDate} to {EndDate})",
            technicianId, statusId ?? -1, start, end);

        return await _repository.GetMyWorkOrdersAsync(
            technicianId, statusId, start, end, cancellationToken);
    }

    public async Task<TechnicianPerformanceDto> GetMyPerformanceAsync(
        int technicianId,
        DateTime? periodStart = null,
        DateTime? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        // Reuse existing performance method
        return await GetPerformanceAsync(technicianId, periodStart, periodEnd, cancellationToken);
    }

    public async Task<List<TechnicianRatingResponseDto>> GetMyRatingsAsync(
        int technicianId,
        int? minRating = null,
        DateOnly? startDate = null,
        DateOnly? endDate = null,
        CancellationToken cancellationToken = default)
    {
        // Default: All ratings from last 90 days
        var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-90));
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        _logger.LogInformation(
            "Technician {TechnicianId} viewing own ratings (MinRating: {MinRating}, {StartDate} to {EndDate})",
            technicianId, minRating ?? 0, start, end);

        // Validate min rating
        if (minRating.HasValue && (minRating < 1 || minRating > 5))
            throw new ArgumentException("Min rating must be between 1 and 5", nameof(minRating));

        return await _repository.GetMyRatingsAsync(
            technicianId, minRating, start, end, cancellationToken);
    }

    public async Task<bool> RequestTimeOffAsync(
        RequestTimeOffDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Technician {TechnicianId} requesting time off from {StartDate} to {EndDate} ({Type})",
            request.TechnicianId, request.StartDate, request.EndDate, request.TimeOffType);

        // Validate dates
        if (request.EndDate < request.StartDate)
            throw new ArgumentException("End date must be after start date");

        if (request.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("Cannot request time off for past dates");

        // Business rule: Max 30 days continuous time off
        var daysOff = (request.EndDate.ToDateTime(TimeOnly.MinValue) - 
                      request.StartDate.ToDateTime(TimeOnly.MinValue)).Days + 1;
        if (daysOff > 30)
            throw new ArgumentException("Time off period cannot exceed 30 days");

        // Business rule: Minimum 3 days notice (except emergency)
        if (request.TimeOffType != "Emergency")
        {
            var daysNotice = (request.StartDate.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days;
            if (daysNotice < 3)
                throw new ArgumentException("Time off requests require at least 3 days notice (except emergencies)");
        }

        return await _repository.RequestTimeOffAsync(request, cancellationToken);
    }

    #endregion
}
