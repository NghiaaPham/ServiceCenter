using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVServiceCenter.Core.Domains.Customers.DTOs.Responses
{
    public class CustomerResponseDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public int? TypeId { get; set; }
        public string PreferredLanguage { get; set; } = string.Empty;
        public bool? MarketingOptIn { get; set; }
        public int? LoyaltyPoints { get; set; }
        public decimal? TotalSpent { get; set; }
        public DateOnly? LastVisitDate { get; set; }
        public string? Notes { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }

        // Navigation properties
        public CustomerTypeResponseDto? CustomerType { get; set; }

        // Computed properties
        public int Age { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string ContactInfo { get; set; } = string.Empty;
        public string LoyaltyStatus { get; set; } = string.Empty;
        public int VehicleCount { get; set; }
        public int ActiveVehicleCount { get; set; }
        public decimal PotentialDiscount { get; set; }
        public string LastVisitStatus { get; set; } = string.Empty;
        public List<CustomerVehicleSummaryDto> RecentVehicles { get; set; } = new();
    }
}
