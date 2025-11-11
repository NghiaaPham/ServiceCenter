namespace EVServiceCenter.Core.Domains.ServiceRatings.DTOs.Responses;

/// <summary>
/// Service rating response DTO
/// </summary>
public class ServiceRatingResponseDto
{
    public int RatingId { get; set; }
    public int WorkOrderId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public int? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }
    public int? AdvisorId { get; set; }
    public string? AdvisorName { get; set; }

    public int? OverallRating { get; set; }
    public int? ServiceQuality { get; set; }
    public int? StaffProfessionalism { get; set; }
    public int? FacilityQuality { get; set; }
    public int? WaitingTime { get; set; }
    public int? PriceValue { get; set; }
    public int? CommunicationQuality { get; set; }

    public string? PositiveFeedback { get; set; }
    public string? NegativeFeedback { get; set; }
    public string? Suggestions { get; set; }

    public bool? WouldRecommend { get; set; }
    public bool? WouldReturn { get; set; }
    public DateTime? RatingDate { get; set; }
    public bool? IsVerified { get; set; }
}

/// <summary>
/// Service center ratings summary
/// </summary>
public class ServiceCenterRatingsResponseDto
{
    public int ServiceCenterId { get; set; }
    public string ServiceCenterName { get; set; } = null!;
    public int TotalRatings { get; set; }
    public decimal AverageOverallRating { get; set; }
    public decimal AverageServiceQuality { get; set; }
    public decimal AverageStaffProfessionalism { get; set; }
    public decimal AverageFacilityQuality { get; set; }
    public decimal AverageWaitingTime { get; set; }
    public decimal AveragePriceValue { get; set; }
    public decimal AverageCommunicationQuality { get; set; }
    public int RecommendCount { get; set; }
    public decimal RecommendPercentage { get; set; }
    public int WouldReturnCount { get; set; }
    public decimal WouldReturnPercentage { get; set; }
    public List<ServiceRatingResponseDto> RecentRatings { get; set; } = new();
}

/// <summary>
/// Rating distribution breakdown
/// </summary>
public class RatingDistributionDto
{
    public int Stars { get; set; }
    public int Count { get; set; }
    public decimal Percentage { get; set; }
}
