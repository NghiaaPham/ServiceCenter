namespace EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Responses
{
    public class ModelServicePricingResponseDto
    {
        public int PricingId { get; set; }
        public int ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public decimal? CustomPrice { get; set; }
        public decimal BasePrice { get; set; }
        public decimal FinalPrice { get; set; }
        public int? CustomTime { get; set; }
        public int StandardTime { get; set; }
        public int FinalTime { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
        public bool IsCurrentlyActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}