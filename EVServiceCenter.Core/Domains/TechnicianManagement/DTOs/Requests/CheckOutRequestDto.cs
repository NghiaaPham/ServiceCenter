namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests
{
    /// <summary>
    /// Request DTO for technician check-out
    /// </summary>
    public class CheckOutRequestDto
    {
        /// <summary>
        /// Optional notes for check-out
        /// Example: "Leaving early for medical appointment"
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Optional reason for early checkout
        /// Only required if checking out before scheduled end time
        /// </summary>
        public string? EarlyCheckoutReason { get; set; }
    }
}
