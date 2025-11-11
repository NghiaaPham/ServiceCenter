namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses
{
    /// <summary>
    /// Response DTO for technician's schedule
    /// </summary>
    public class TechnicianScheduleResponseDto
    {
        public int ScheduleId { get; set; }
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        
        public int CenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;
        
        public DateOnly WorkDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public TimeOnly? BreakStartTime { get; set; }
        public TimeOnly? BreakEndTime { get; set; }
        
        public int? MaxCapacityMinutes { get; set; }
        public int? BookedMinutes { get; set; }
        public int? AvailableMinutes { get; set; }
        
        public bool? IsAvailable { get; set; }
        public string? ShiftType { get; set; }
        public string? Notes { get; set; }
        
        // Computed
        public decimal UtilizationRate => MaxCapacityMinutes > 0 
            ? Math.Round((decimal)(BookedMinutes ?? 0) / MaxCapacityMinutes.Value * 100, 2) 
            : 0;
    }
}
