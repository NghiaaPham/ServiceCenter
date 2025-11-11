namespace EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Requests
{
    public class MaintenanceServiceQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchTerm { get; set; }
        public int? CategoryId { get; set; }
        public int? ModelId { get; set; } // Filter services by vehicle model
        public bool? IsActive { get; set; }
        public bool? IsWarrantyService { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SkillLevel { get; set; }
        public string SortBy { get; set; } = "ServiceName";
        public string SortOrder { get; set; } = "asc";
    }
}