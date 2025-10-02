namespace EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests
{
    public class TimeSlotQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? CenterId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool? IsBlocked { get; set; }
        public bool? OnlyAvailable { get; set; }
        public string SortBy { get; set; } = "SlotDate";
        public string SortOrder { get; set; } = "asc";
    }
}