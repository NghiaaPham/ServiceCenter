namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses
{
    /// <summary>
    /// Response DTO for technician's ratings from customers
    /// </summary>
    public class TechnicianRatingResponseDto
    {
        public int RatingId { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderCode { get; set; } = string.Empty;
        
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        
        // Ratings (1-5 stars)
        public int? OverallRating { get; set; }
        public int? ServiceQuality { get; set; }
        public int? StaffProfessionalism { get; set; }
        public int? CommunicationQuality { get; set; }
        
        // Feedback
        public string? PositiveFeedback { get; set; }
        public string? NegativeFeedback { get; set; }
        public string? Suggestions { get; set; }
        
        public bool? WouldRecommend { get; set; }
        public DateTime? RatingDate { get; set; }
        
        // Response from staff
        public string? ResponseText { get; set; }
        public DateTime? ResponseDate { get; set; }
    }
}
