namespace EVServiceCenter.Core.Domains.Customers.DTOs.Requests
{
    public class UpdateCustomerProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Address { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string PreferredLanguage { get; set; } = "vi-VN";
        public bool MarketingOptIn { get; set; }
    }
}
