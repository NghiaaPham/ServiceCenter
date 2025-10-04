namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
    public class CreateAppointmentRequestDto
    {
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public int ServiceCenterId { get; set; }
        public int SlotId { get; set; }

        /// <summary>
        /// Gói dịch vụ (optional)
        /// </summary>
        public int? PackageId { get; set; }

        /// <summary>
        /// Danh sách dịch vụ đơn lẻ (có thể là extra services nếu có PackageId)
        /// </summary>
        public List<int> ServiceIds { get; set; } = new();

        public string? CustomerNotes { get; set; }
        public int? PreferredTechnicianId { get; set; }
        public string Priority { get; set; } = "Normal"; // Normal, High, Urgent
        public string Source { get; set; } = "Online"; // Online, Walk-in, Phone
    }
}
