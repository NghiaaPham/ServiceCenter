namespace EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Requests
{
    public class CreateModelServicePricingRequestDto
    {
        public int ModelId { get; set; }
        public int ServiceId { get; set; }
        public decimal? CustomPrice { get; set; }
        public int? CustomTime { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateOnly? EffectiveDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }
    }
}