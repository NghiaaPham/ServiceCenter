namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses
{
    /// <summary>
    /// Summary DTO for technician schedule
    /// Used in list views and brief displays
    /// </summary>
    public class TechnicianScheduleSummaryDto
    {
        public int ScheduleId { get; set; }
        public DateOnly WorkDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? ShiftType { get; set; }
        public int? AvailableMinutes { get; set; }
        public int? BookedMinutes { get; set; }
        public bool IsAvailable { get; set; }
    }
}
