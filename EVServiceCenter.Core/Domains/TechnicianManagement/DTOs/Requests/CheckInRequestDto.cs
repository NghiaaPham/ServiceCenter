namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests
{
    /// <summary>
    /// Request DTO for technician check-in
    /// </summary>
    public class CheckInRequestDto
    {
        /// <summary>
        /// Service center where technician is checking in
        /// Required for multi-center support
        /// </summary>
        public int ServiceCenterId { get; set; }

        /// <summary>
        /// Optional shift type override
        /// If null, system will auto-detect from TechnicianSchedule
        /// Valid values: Morning, Afternoon, Evening, FullDay
        /// </summary>
        public string? ShiftType { get; set; }

        /// <summary>
        /// Optional notes for check-in
        /// Example: "Late due to traffic", "Early arrival"
        /// </summary>
        public string? Notes { get; set; }
    }
}
