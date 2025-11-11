namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests
{
    /// <summary>
    /// Request DTO for technician to request time off
    /// </summary>
    public class RequestTimeOffDto
    {
        public int TechnicianId { get; set; }
        
        public DateOnly StartDate { get; set; }
        
        public DateOnly EndDate { get; set; }
        
        public string Reason { get; set; } = string.Empty;
        
        /// <summary>
        /// TimeOffType: Sick, Vacation, Personal, Emergency
        /// </summary>
        public string TimeOffType { get; set; } = "Personal";
        
        public string? Notes { get; set; }
    }
}
