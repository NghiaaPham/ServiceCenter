namespace EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Requests
{
    public class UpdateServiceCategoryRequestDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; }
    }
}