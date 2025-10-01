namespace EVServiceCenter.Core.Domains.CarModels.DTOs.Requests
{
    public class CarModelQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Filtering
        public string? SearchTerm { get; set; }
        public int? BrandId { get; set; }
        public int? Year { get; set; }
        public bool? IsActive { get; set; }

        // Sorting
        public string SortBy { get; set; } = "ModelName";
        public string SortOrder { get; set; } = "asc";

        // Include options
        public bool IncludeStats { get; set; } = false;
        public bool IncludeBrand { get; set; } = true;
    }
}