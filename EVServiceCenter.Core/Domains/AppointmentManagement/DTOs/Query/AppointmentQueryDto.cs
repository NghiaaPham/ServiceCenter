namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query
{
    public class AppointmentQueryDto
    {
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Filters
        public int? CustomerId { get; set; }
        public int? ServiceCenterId { get; set; }
        public int? StatusId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Priority { get; set; }
        public string? Source { get; set; }
        public string? SearchTerm { get; set; }

        // Sorting
        public string SortBy { get; set; } = "AppointmentDate";
        public string SortOrder { get; set; } = "desc";

        // Computed
        public int Skip => (Page - 1) * PageSize;
    }
}
