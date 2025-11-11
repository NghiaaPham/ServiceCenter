namespace EVServiceCenter.Core.Domains.ServiceRatings.DTOs.Requests;

/// <summary>
/// Request DTO for creating service rating
/// </summary>
public class CreateServiceRatingRequestDto
{
    /// <summary>
    /// WorkOrder ID being rated
    /// </summary>
    public int WorkOrderId { get; set; }

    /// <summary>
    /// Overall rating (1-5 stars)
    /// </summary>
    public int OverallRating { get; set; }

    /// <summary>
    /// Service quality rating (1-5 stars)
    /// </summary>
    public int? ServiceQuality { get; set; }

    /// <summary>
    /// Staff professionalism rating (1-5 stars)
    /// </summary>
    public int? StaffProfessionalism { get; set; }

    /// <summary>
    /// Facility quality rating (1-5 stars)
    /// </summary>
    public int? FacilityQuality { get; set; }

    /// <summary>
    /// Waiting time satisfaction (1-5 stars)
    /// </summary>
    public int? WaitingTime { get; set; }

    /// <summary>
    /// Price-value ratio (1-5 stars)
    /// </summary>
    public int? PriceValue { get; set; }

    /// <summary>
    /// Communication quality (1-5 stars)
    /// </summary>
    public int? CommunicationQuality { get; set; }

    /// <summary>
    /// Positive feedback/comments
    /// </summary>
    public string? PositiveFeedback { get; set; }

    /// <summary>
    /// Negative feedback/complaints
    /// </summary>
    public string? NegativeFeedback { get; set; }

    /// <summary>
    /// Suggestions for improvement
    /// </summary>
    public string? Suggestions { get; set; }

    /// <summary>
    /// Would recommend to others
    /// </summary>
    public bool? WouldRecommend { get; set; }

    /// <summary>
    /// Would return for future service
    /// </summary>
    public bool? WouldReturn { get; set; }
}
