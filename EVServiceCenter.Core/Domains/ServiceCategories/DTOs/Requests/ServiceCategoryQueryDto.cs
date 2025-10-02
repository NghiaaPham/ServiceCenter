namespace EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Requests
{
    public class ServiceCategoryQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "DisplayOrder";
        public string SortOrder { get; set; } = "asc";
    }
}