namespace EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests
{
    public class CreateTimeSlotRequestDto
    {
        public int CenterId { get; set; }
        public DateOnly SlotDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int MaxBookings { get; set; } = 1;
        public string? SlotType { get; set; }
        public bool IsBlocked { get; set; } = false;
        public string? Notes { get; set; }
    }
}