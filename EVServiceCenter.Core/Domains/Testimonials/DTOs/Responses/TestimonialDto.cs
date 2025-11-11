namespace EVServiceCenter.Core.Domains.Testimonials.DTOs.Responses
{
    public class TestimonialDto
    {
        public int RatingId { get; set; }
        public int WorkOrderId { get; set; }
        public string? CustomerName { get; set; }
        public int? OverallRating { get; set; }
        public string? PositiveFeedback { get; set; }
        public string? NegativeFeedback { get; set; }
        public string? Suggestions { get; set; }
        public DateTime? RatingDate { get; set; }
        public string? ServiceCenterName { get; set; }
        public string? VehicleModelName { get; set; }
    }
}