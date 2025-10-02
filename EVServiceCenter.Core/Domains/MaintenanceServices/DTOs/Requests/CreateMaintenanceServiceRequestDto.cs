namespace EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Requests
{
    public class CreateMaintenanceServiceRequestDto
    {
        public int CategoryId { get; set; }
        public string ServiceCode { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StandardTime { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? LaborCost { get; set; }
        public string? SkillLevel { get; set; }
        public string? RequiredCertification { get; set; }
        public bool IsWarrantyService { get; set; } = false;
        public int? WarrantyPeriod { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}