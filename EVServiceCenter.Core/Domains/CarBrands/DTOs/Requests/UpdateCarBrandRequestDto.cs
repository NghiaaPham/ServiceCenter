namespace EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests
{
    public class UpdateCarBrandRequestDto
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}