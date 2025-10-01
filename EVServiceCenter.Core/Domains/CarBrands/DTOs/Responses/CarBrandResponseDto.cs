namespace EVServiceCenter.Core.Domains.CarBrands.DTOs.Responses
{
    public class CarBrandResponseDto
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? LogoUrl { get; set; }
        public string? Website { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Statistics (optional)
        public int? TotalModels { get; set; }
        public int? ActiveModels { get; set; }
        public int? TotalVehicles { get; set; }
    }
}