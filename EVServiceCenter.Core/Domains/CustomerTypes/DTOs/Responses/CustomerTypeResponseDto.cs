namespace EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Responses
{
    public class CustomerTypeResponseDto
    {
        public int TypeId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public decimal DiscountPercent { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public int CustomerCount { get; set; }
        public int ActiveCustomerCount { get; set; }
        public decimal TotalRevenueFromType { get; set; }
    }
}
