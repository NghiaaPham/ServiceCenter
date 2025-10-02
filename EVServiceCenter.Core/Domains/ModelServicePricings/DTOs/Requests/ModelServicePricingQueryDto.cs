namespace EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Requests
{
    public class ModelServicePricingQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? ModelId { get; set; }
        public int? ServiceId { get; set; }
        public bool? IsActive { get; set; }
        public DateOnly? EffectiveDate { get; set; }
        public string SortBy { get; set; } = "PricingId";
        public string SortOrder { get; set; } = "asc";
    }
}