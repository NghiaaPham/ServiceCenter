namespace EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Responses
{
    public class ServiceCategoryResponseDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Statistics
        public int ServiceCount { get; set; }
        public int ActiveServiceCount { get; set; }
    }
}