using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.ServiceRatings.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceRatings.DTOs.Responses;
using EVServiceCenter.Core.Domains.ServiceRatings.Interfaces;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.ServiceRatings.Services;

/// <summary>
/// Service rating service implementation
/// Handles rating creation and aggregation with performance optimization
/// </summary>
public class ServiceRatingService : IServiceRatingService
{
    private readonly EVDbContext _context;
    private readonly ILogger<ServiceRatingService> _logger;

    public ServiceRatingService(
        EVDbContext context,
        ILogger<ServiceRatingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ServiceRatingResponseDto> CreateRatingAsync(
        CreateServiceRatingRequestDto request,
        int customerId,
        CancellationToken cancellationToken = default)
    {
        // Validate work order exists and belongs to customer
        var workOrder = await _context.Set<WorkOrder>()
            .Include(wo => wo.ServiceRatings)
            .Include(wo => wo.Status)
            .FirstOrDefaultAsync(wo => wo.WorkOrderId == request.WorkOrderId,
                cancellationToken);

        if (workOrder == null)
        {
            throw new KeyNotFoundException($"WorkOrder {request.WorkOrderId} not found");
        }

        // ? CRITICAL: Verify customer owns this work order
        if (workOrder.CustomerId != customerId)
        {
            throw new InvalidOperationException(
                "You can only rate work orders for services you received. " +
                "This work order belongs to another customer.");
        }

        // Check if already rated
        if (workOrder.ServiceRatings.Any())
        {
            throw new InvalidOperationException("This work order has already been rated");
        }

        // Check if work order is completed
        if (workOrder.Status?.StatusName != "Completed")
        {
            throw new InvalidOperationException(
                $"Can only rate completed work orders. Current status: {workOrder.Status?.StatusName ?? "Unknown"}");
        }

        // ? CRITICAL: Check quality check requirement
        if (workOrder.QualityCheckRequired == true && !workOrder.QualityCheckedBy.HasValue)
        {
            throw new InvalidOperationException(
                "Quality check must be completed by staff before you can rate this work order. " +
                "Please contact the service center for assistance.");
        }

        // Validate ratings are in range 1-5
        ValidateRatingRange(request.OverallRating, nameof(request.OverallRating));
        if (request.ServiceQuality.HasValue)
            ValidateRatingRange(request.ServiceQuality.Value, nameof(request.ServiceQuality));
        if (request.StaffProfessionalism.HasValue)
            ValidateRatingRange(request.StaffProfessionalism.Value, nameof(request.StaffProfessionalism));
        if (request.FacilityQuality.HasValue)
            ValidateRatingRange(request.FacilityQuality.Value, nameof(request.FacilityQuality));
        if (request.WaitingTime.HasValue)
            ValidateRatingRange(request.WaitingTime.Value, nameof(request.WaitingTime));
        if (request.PriceValue.HasValue)
            ValidateRatingRange(request.PriceValue.Value, nameof(request.PriceValue));
        if (request.CommunicationQuality.HasValue)
            ValidateRatingRange(request.CommunicationQuality.Value, nameof(request.CommunicationQuality));

        // Create rating
        var rating = new ServiceRating
        {
            WorkOrderId = request.WorkOrderId,
            CustomerId = customerId,
            TechnicianId = workOrder.TechnicianId,
            AdvisorId = workOrder.AdvisorId,
            OverallRating = request.OverallRating,
            ServiceQuality = request.ServiceQuality,
            StaffProfessionalism = request.StaffProfessionalism,
            FacilityQuality = request.FacilityQuality,
            WaitingTime = request.WaitingTime,
            PriceValue = request.PriceValue,
            CommunicationQuality = request.CommunicationQuality,
            PositiveFeedback = request.PositiveFeedback,
            NegativeFeedback = request.NegativeFeedback,
            Suggestions = request.Suggestions,
            WouldRecommend = request.WouldRecommend,
            WouldReturn = request.WouldReturn,
            RatingDate = DateTime.UtcNow,
            IsVerified = true // Auto-verify for now
        };

        _context.Set<ServiceRating>().Add(rating);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Rating created for WorkOrder {WorkOrderId} by Customer {CustomerId}. Overall: {Rating}/5",
            request.WorkOrderId, customerId, request.OverallRating);

        // Fetch complete rating with navigation properties
        return await GetRatingByIdAsync(rating.RatingId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve created rating");
    }

    public async Task<ServiceRatingResponseDto?> GetRatingByIdAsync(
        int ratingId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ServiceRating>()
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Technician)
            .Include(r => r.Advisor)
            .Where(r => r.RatingId == ratingId)
            .Select(r => new ServiceRatingResponseDto
            {
                RatingId = r.RatingId,
                WorkOrderId = r.WorkOrderId,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer.FullName ?? "Unknown",
                TechnicianId = r.TechnicianId,
                TechnicianName = r.Technician != null ? r.Technician.FullName : null,
                AdvisorId = r.AdvisorId,
                AdvisorName = r.Advisor != null ? r.Advisor.FullName : null,
                OverallRating = r.OverallRating,
                ServiceQuality = r.ServiceQuality,
                StaffProfessionalism = r.StaffProfessionalism,
                FacilityQuality = r.FacilityQuality,
                WaitingTime = r.WaitingTime,
                PriceValue = r.PriceValue,
                CommunicationQuality = r.CommunicationQuality,
                PositiveFeedback = r.PositiveFeedback,
                NegativeFeedback = r.NegativeFeedback,
                Suggestions = r.Suggestions,
                WouldRecommend = r.WouldRecommend,
                WouldReturn = r.WouldReturn,
                RatingDate = r.RatingDate,
                IsVerified = r.IsVerified
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceRatingResponseDto?> GetRatingByWorkOrderIdAsync(
        int workOrderId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<ServiceRating>()
            .AsNoTracking()
            .Include(r => r.Customer)
            .Include(r => r.Technician)
            .Include(r => r.Advisor)
            .Where(r => r.WorkOrderId == workOrderId)
            .Select(r => new ServiceRatingResponseDto
            {
                RatingId = r.RatingId,
                WorkOrderId = r.WorkOrderId,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer.FullName ?? "Unknown",
                TechnicianId = r.TechnicianId,
                TechnicianName = r.Technician != null ? r.Technician.FullName : null,
                AdvisorId = r.AdvisorId,
                AdvisorName = r.Advisor != null ? r.Advisor.FullName : null,
                OverallRating = r.OverallRating,
                ServiceQuality = r.ServiceQuality,
                StaffProfessionalism = r.StaffProfessionalism,
                FacilityQuality = r.FacilityQuality,
                WaitingTime = r.WaitingTime,
                PriceValue = r.PriceValue,
                CommunicationQuality = r.CommunicationQuality,
                PositiveFeedback = r.PositiveFeedback,
                NegativeFeedback = r.NegativeFeedback,
                Suggestions = r.Suggestions,
                WouldRecommend = r.WouldRecommend,
                WouldReturn = r.WouldReturn,
                RatingDate = r.RatingDate,
                IsVerified = r.IsVerified
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceCenterRatingsResponseDto> GetServiceCenterRatingsAsync(
        int serviceCenterId,
        int? top = 10,
        CancellationToken cancellationToken = default)
    {
        // Get service center info
        var serviceCenter = await _context.Set<ServiceCenter>()
            .AsNoTracking()
            .FirstOrDefaultAsync(sc => sc.CenterId == serviceCenterId, cancellationToken);

        if (serviceCenter == null)
        {
            throw new KeyNotFoundException($"ServiceCenter {serviceCenterId} not found");
        }

        // Get all ratings for this service center
        var ratings = await _context.Set<ServiceRating>()
            .AsNoTracking()
            .Include(r => r.WorkOrder)
            .Include(r => r.Customer)
            .Include(r => r.Technician)
            .Include(r => r.Advisor)
            .Where(r => r.WorkOrder.ServiceCenterId == serviceCenterId)
            .ToListAsync(cancellationToken);

        if (!ratings.Any())
        {
            return new ServiceCenterRatingsResponseDto
            {
                ServiceCenterId = serviceCenterId,
                ServiceCenterName = serviceCenter.CenterName,
                TotalRatings = 0,
                RecentRatings = new List<ServiceRatingResponseDto>()
            };
        }

        // Calculate averages
        var totalRatings = ratings.Count;
        var avgOverall = (decimal)Math.Round(ratings.Average(r => r.OverallRating ?? 0), 2);
        var avgServiceQuality = (decimal)Math.Round(ratings.Where(r => r.ServiceQuality.HasValue).Average(r => r.ServiceQuality!.Value), 2);
        var avgStaffProf = (decimal)Math.Round(ratings.Where(r => r.StaffProfessionalism.HasValue).Average(r => r.StaffProfessionalism!.Value), 2);
        var avgFacility = (decimal)Math.Round(ratings.Where(r => r.FacilityQuality.HasValue).Average(r => r.FacilityQuality!.Value), 2);
        var avgWaiting = (decimal)Math.Round(ratings.Where(r => r.WaitingTime.HasValue).Average(r => r.WaitingTime!.Value), 2);
        var avgPrice = (decimal)Math.Round(ratings.Where(r => r.PriceValue.HasValue).Average(r => r.PriceValue!.Value), 2);
        var avgComm = (decimal)Math.Round(ratings.Where(r => r.CommunicationQuality.HasValue).Average(r => r.CommunicationQuality!.Value), 2);

        var recommendCount = ratings.Count(r => r.WouldRecommend == true);
        var recommendPercentage = Math.Round((decimal)recommendCount / totalRatings * 100, 2);

        var returnCount = ratings.Count(r => r.WouldReturn == true);
        var returnPercentage = Math.Round((decimal)returnCount / totalRatings * 100, 2);

        // Get recent ratings
        var recentRatings = ratings
            .OrderByDescending(r => r.RatingDate)
            .Take(top ?? 10)
            .Select(r => new ServiceRatingResponseDto
            {
                RatingId = r.RatingId,
                WorkOrderId = r.WorkOrderId,
                CustomerId = r.CustomerId,
                CustomerName = r.Customer.FullName ?? "Unknown",
                TechnicianId = r.TechnicianId,
                TechnicianName = r.Technician != null ? r.Technician.FullName : null,
                AdvisorId = r.AdvisorId,
                AdvisorName = r.Advisor != null ? r.Advisor.FullName : null,
                OverallRating = r.OverallRating,
                ServiceQuality = r.ServiceQuality,
                StaffProfessionalism = r.StaffProfessionalism,
                FacilityQuality = r.FacilityQuality,
                WaitingTime = r.WaitingTime,
                PriceValue = r.PriceValue,
                CommunicationQuality = r.CommunicationQuality,
                PositiveFeedback = r.PositiveFeedback,
                NegativeFeedback = r.NegativeFeedback,
                Suggestions = r.Suggestions,
                WouldRecommend = r.WouldRecommend,
                WouldReturn = r.WouldReturn,
                RatingDate = r.RatingDate,
                IsVerified = r.IsVerified
            })
            .ToList();

        return new ServiceCenterRatingsResponseDto
        {
            ServiceCenterId = serviceCenterId,
            ServiceCenterName = serviceCenter.CenterName,
            TotalRatings = totalRatings,
            AverageOverallRating = avgOverall,
            AverageServiceQuality = avgServiceQuality,
            AverageStaffProfessionalism = avgStaffProf,
            AverageFacilityQuality = avgFacility,
            AverageWaitingTime = avgWaiting,
            AveragePriceValue = avgPrice,
            AverageCommunicationQuality = avgComm,
            RecommendCount = recommendCount,
            RecommendPercentage = recommendPercentage,
            WouldReturnCount = returnCount,
            WouldReturnPercentage = returnPercentage,
            RecentRatings = recentRatings
        };
    }

    public async Task<bool> CanRateWorkOrderAsync(
        int workOrderId,
        int customerId,
        CancellationToken cancellationToken = default)
    {
        var workOrder = await _context.Set<WorkOrder>()
            .AsNoTracking()
            .Include(wo => wo.ServiceRatings)
            .Include(wo => wo.Status)
            .FirstOrDefaultAsync(wo => wo.WorkOrderId == workOrderId,
                cancellationToken);

        if (workOrder == null)
        {
            return false;
        }

        // ? CRITICAL: Verify ownership
        if (workOrder.CustomerId != customerId)
        {
            return false;
        }

        // Must be completed
        if (workOrder.Status?.StatusName != "Completed")
        {
            return false;
        }

        // ? CRITICAL: Quality check must be done (if required)
        if (workOrder.QualityCheckRequired == true && !workOrder.QualityCheckedBy.HasValue)
        {
            return false;
        }

        // Not already rated
        return !workOrder.ServiceRatings.Any();
    }

    private static void ValidateRatingRange(int rating, string fieldName)
    {
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentException($"{fieldName} must be between 1 and 5 stars");
        }
    }
}
