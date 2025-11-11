namespace EVServiceCenter.Core.Domains.Customers.DTOs.Requests
{
    public class CreateCustomerRequestDto
    {
        public string? Username { get; set; } // Username for login account
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? IdentityNumber { get; set; } // Will be encrypted
        public int? TypeId { get; set; }
        public string PreferredLanguage { get; set; } = "vi-VN";
        public bool MarketingOptIn { get; set; } = true;
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
