namespace EVServiceCenter.Core.Domains.Customers.DTOs.Requests
{
    public class CustomerQueryDto
    {
        public string? SearchTerm { get; set; }
        public int? TypeId { get; set; }
        public string? Gender { get; set; }
        public bool? IsActive { get; set; }
        public bool? MarketingOptIn { get; set; }
        public DateOnly? DateOfBirthFrom { get; set; }
        public DateOnly? DateOfBirthTo { get; set; }
        public decimal? TotalSpentFrom { get; set; }
        public decimal? TotalSpentTo { get; set; }
        public int? LoyaltyPointsFrom { get; set; }
        public int? LoyaltyPointsTo { get; set; }
        public DateOnly? LastVisitFrom { get; set; }
        public DateOnly? LastVisitTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "FullName";
        public bool SortDesc { get; set; } = false;
        public bool IncludeStats { get; set; } = false;
    }
}
