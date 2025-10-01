namespace EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Responses
{
    public class ServiceCenterResponseDto
    {
        public int CenterId { get; set; }
        public string CenterCode { get; set; } = string.Empty;
        public string CenterName { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;
        public string? Ward { get; set; }
        public string? District { get; set; }
        public string? Province { get; set; }
        public string? PostalCode { get; set; }
        public string FullAddress { get; set; } = string.Empty; // Computed

        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Website { get; set; }
        public string? ImageUrl { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public TimeOnly OpenTime { get; set; }
        public TimeOnly CloseTime { get; set; }
        public string WorkingHours { get; set; } = string.Empty; // e.g., "08:00 - 18:00"

        public int? Capacity { get; set; }

        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; }

        public bool? IsActive { get; set; }
        public string? Description { get; set; }
        public string? Facilities { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // Statistics (optional, included when IncludeStats = true)
        public int? TotalAppointments { get; set; }
        public int? TotalWorkOrders { get; set; }
        public int? TotalTechnicians { get; set; }
        public int? TotalDepartments { get; set; }
        public int? ActiveTimeSlots { get; set; }
        public decimal? AverageRating { get; set; }
        public decimal? MonthlyRevenue { get; set; }
    }
}
