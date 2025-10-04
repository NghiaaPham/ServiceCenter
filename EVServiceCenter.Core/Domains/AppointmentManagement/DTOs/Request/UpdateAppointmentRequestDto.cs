namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
    public class UpdateAppointmentRequestDto
    {
        /// <summary>
        /// ID appointment cần update
        /// </summary>
        public int AppointmentId { get; set; }

        /// <summary>
        /// ID xe (có thể đổi xe khác của cùng khách hàng)
        /// </summary>
        public int? VehicleId { get; set; }

        /// <summary>
        /// ID slot (có thể đổi thời gian)
        /// </summary>
        public int? SlotId { get; set; }

        /// <summary>
        /// Gói dịch vụ (có thể thay đổi hoặc bỏ)
        /// </summary>
        public int? PackageId { get; set; }

        /// <summary>
        /// Danh sách dịch vụ (thay thế hoàn toàn danh sách cũ)
        /// </summary>
        public List<int>? ServiceIds { get; set; }

        /// <summary>
        /// Ghi chú khách hàng
        /// </summary>
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Mô tả dịch vụ
        /// </summary>
        public string? ServiceDescription { get; set; }

        /// <summary>
        /// Kỹ thuật viên ưu tiên
        /// </summary>
        public int? PreferredTechnicianId { get; set; }

        /// <summary>
        /// Độ ưu tiên: Normal, High, Urgent
        /// </summary>
        public string? Priority { get; set; }
    }
}
