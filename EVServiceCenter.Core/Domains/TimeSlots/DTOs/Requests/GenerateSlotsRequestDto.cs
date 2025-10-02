namespace EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests
{
    public class GenerateSlotsRequestDto
    {
        public int CenterId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public int SlotDurationMinutes { get; set; } = 60;
        public int MaxBookingsPerSlot { get; set; } = 1;
        public string? SlotType { get; set; }
        public bool OverwriteExisting { get; set; } = false;
    }
}