namespace EVServiceCenter.Core.Domains.Identity.DTOs
{
    public class TokenCustomerInfo
    {
        public int CustomerId { get; set; }
        public string? CustomerCode { get; set; }
        public int? CustomerTypeId { get; set; }
        public int LoyaltyPoints { get; set; }
    }
}