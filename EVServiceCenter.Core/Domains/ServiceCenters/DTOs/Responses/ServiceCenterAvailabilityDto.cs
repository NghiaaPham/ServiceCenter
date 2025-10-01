namespace EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Responses
{
    public class ServiceCenterAvailabilityDto
    {
        public int CenterId { get; set; }
        public string CenterName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentBookings { get; set; }
        public int AvailableSlots { get; set; }
        public bool IsAvailable { get; set; }
        public decimal UtilizationRate { get; set; } // Percentage
        public DateTime Date { get; set; }
    }
}