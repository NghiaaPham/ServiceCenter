namespace EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses
{
    /// <summary>
    /// Response DTO for shift/attendance information
    /// </summary>
    public class ShiftResponseDto
    {
        public int ShiftId { get; set; }
        public int TechnicianId { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        
        public int ServiceCenterId { get; set; }
        public string ServiceCenterName { get; set; } = string.Empty;
        
        public DateOnly ShiftDate { get; set; }
        public string ShiftType { get; set; } = string.Empty;
        
        // Attendance timestamps
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        
        // Calculated hours
        public decimal? WorkedHours { get; set; }
        public decimal? NetWorkingHours { get; set; }
        
        // Compliance flags
        public bool IsLate { get; set; }
        public bool IsEarlyLeave { get; set; }
        
        // Status: Present, Absent, OnLeave, Sick, etc.
        public string Status { get; set; } = string.Empty;
        
        // Optional notes
        public string? Notes { get; set; }
        
        // Schedule info for comparison
        public TimeOnly? ScheduledStartTime { get; set; }
        public TimeOnly? ScheduledEndTime { get; set; }
        
        // Computed properties
        public bool IsCheckedIn => CheckInTime.HasValue && !CheckOutTime.HasValue;
        public bool IsCompleted => CheckInTime.HasValue && CheckOutTime.HasValue;
        
        public int? LateMinutes
        {
            get
            {
                if (!IsLate || !CheckInTime.HasValue || !ScheduledStartTime.HasValue)
                    return null;
                
                // ? CheckInTime ?ã là VN time (converted trong MapToResponseDto)
                // ? ScheduledStartTime là TimeOnly (gi? theo VN timezone)
                // ? Combine ShiftDate + ScheduledStartTime = DateTime VN (unspecified kind)
                var scheduledStart = ShiftDate.ToDateTime(ScheduledStartTime.Value);
                var actualCheckIn = CheckInTime.Value;
                
                // Tính s? phút mu?n
                var lateMinutes = (actualCheckIn - scheduledStart).TotalMinutes;
                
                // Return only if positive (?úng là mu?n)
                return lateMinutes > 0 ? (int)lateMinutes : 0;
            }
        }
    }
}
