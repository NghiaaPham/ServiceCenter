namespace EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Responses
{
    public class MaintenanceServiceResponseDto
    {
        public int ServiceId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ServiceCode { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int StandardTime { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? LaborCost { get; set; }
        public decimal? TotalCost { get; set; }
        public string? SkillLevel { get; set; }
        public string? RequiredCertification { get; set; }
        public bool IsWarrantyService { get; set; }
        public int? WarrantyPeriod { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Statistics
        public int AppointmentCount { get; set; }
        public int WorkOrderCount { get; set; }
    }
}