namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
    public class AppointmentServiceDto
    {
        public int AppointmentServiceId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; } = null!;
        public string ServiceName { get; set; } = null!;
        public string ServiceSource { get; set; } = null!; // "Package" | "Extra"
        public decimal Price { get; set; }
        public int EstimatedTime { get; set; }
        public string? Notes { get; set; }
    }
}
