namespace EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests
{
    public class UpdateTimeSlotRequestDto
    {
        public int SlotId { get; set; }
        public int CenterId { get; set; }
        public DateOnly SlotDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int MaxBookings { get; set; }
        public string? SlotType { get; set; }
        public bool IsBlocked { get; set; }
        public string? Notes { get; set; }
    }
}