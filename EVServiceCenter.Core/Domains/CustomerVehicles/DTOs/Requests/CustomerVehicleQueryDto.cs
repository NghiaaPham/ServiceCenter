namespace EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests
{
    public class CustomerVehicleQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public string? SearchTerm { get; set; }  
        public int? CustomerId { get; set; }
        public int? ModelId { get; set; }
        public int? BrandId { get; set; }
        public bool? IsActive { get; set; }
        public bool? MaintenanceDue { get; set; }
        public bool? InsuranceExpiring { get; set; } 

        public string SortBy { get; set; } = "LicensePlate";
        public string SortOrder { get; set; } = "asc";

        // Include options
        public bool IncludeCustomer { get; set; } = true;
        public bool IncludeModel { get; set; } = true;
        public bool IncludeStats { get; set; } = false;
    }
}