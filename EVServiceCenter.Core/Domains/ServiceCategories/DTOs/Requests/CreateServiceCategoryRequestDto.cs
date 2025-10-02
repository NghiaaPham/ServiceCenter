namespace EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Requests
{
    public class CreateServiceCategoryRequestDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}