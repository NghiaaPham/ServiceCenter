namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
    public class CreateAppointmentRequestDto
    {
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public int ServiceCenterId { get; set; }
        public int SlotId { get; set; }

        /// <summary>
        /// Gói dịch vụ (optional) - DEPRECATED, sử dụng SubscriptionId thay thế
        /// </summary>
        public int? PackageId { get; set; }

        /// <summary>
        /// Subscription ID - Customer dùng subscription đã mua để book appointment
        /// Nếu có SubscriptionId, services sẽ được lấy từ subscription
        /// </summary>
        public int? SubscriptionId { get; set; }

        /// <summary>
        /// Danh sách dịch vụ đơn lẻ (có thể là extra services nếu có PackageId hoặc SubscriptionId)
        /// </summary>
        public List<int> ServiceIds { get; set; } = new();

        /// <summary>
        /// Mã khuyến mãi (optional) - Nếu có, hệ thống sẽ validate và apply discount
        /// </summary>
        public string? PromotionCode { get; set; }

        public string? CustomerNotes { get; set; }
        public int? PreferredTechnicianId { get; set; }
        public string Priority { get; set; } = "Normal"; // Normal, High, Urgent
        public string Source { get; set; } = "Online"; // Online, Walk-in, Phone
    }
}
