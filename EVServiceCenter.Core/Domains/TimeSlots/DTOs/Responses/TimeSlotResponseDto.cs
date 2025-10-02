namespace EVServiceCenter.Core.Domains.TimeSlots.DTOs.Responses
{
    public class TimeSlotResponseDto
    {
        public int SlotId { get; set; }
        public int CenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public DateOnly SlotDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public int MaxBookings { get; set; }
        public int CurrentBookings { get; set; }
        public int RemainingCapacity { get; set; }
        public string? SlotType { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsAvailable { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}