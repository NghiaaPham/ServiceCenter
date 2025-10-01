namespace EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Requests
{
    public class CreateServiceCenterRequestDto
    {
        public string CenterName { get; set; } = string.Empty;
        public string CenterCode { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Website { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public TimeOnly OpenTime { get; set; } = new TimeOnly(8, 0);
        public TimeOnly CloseTime { get; set; } = new TimeOnly(18, 0);
        public int Capacity { get; set; } = 10;
        public int? ManagerId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; }
        public string? Facilities { get; set; }
    }
}