namespace EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests
{
    public class CarBrandQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Filtering
        public string? SearchTerm { get; set; }
        public string? Country { get; set; }
        public bool? IsActive { get; set; }

        // Sorting
        public string SortBy { get; set; } = "BrandName";
        public string SortOrder { get; set; } = "asc";

        // Include options
        public bool IncludeStats { get; set; } = false;
    }
}