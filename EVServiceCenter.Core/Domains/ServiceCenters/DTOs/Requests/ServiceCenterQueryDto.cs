namespace EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Requests
{
    public class ServiceCenterQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Filtering
        public string? SearchTerm { get; set; }
        public string? Province { get; set; }
        public string? District { get; set; }
        public bool? IsActive { get; set; }

        // Sorting
        public string SortBy { get; set; } = "CenterName";
        public string SortOrder { get; set; } = "asc"; 

        // Include options
        public bool IncludeStats { get; set; } = false;
    }
}
