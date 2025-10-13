using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Core.Interfaces.Services
{
    /// <summary>
    /// Service để audit trail cho mọi thay đổi ServiceSource
    /// Dùng cho transparency và dispute resolution
    /// </summary>
    public interface IServiceSourceAuditService
    {
        /// <summary>
        /// Log một thay đổi ServiceSource của AppointmentService
        /// </summary>
        /// <param name="appointmentServiceId">ID của AppointmentService bị thay đổi</param>
        /// <param name="oldServiceSource">ServiceSource cũ (null nếu tạo mới)</param>
        /// <param name="newServiceSource">ServiceSource mới (Subscription, Extra, Regular)</param>
        /// <param name="oldPrice">Giá cũ</param>
        /// <param name="newPrice">Giá mới</param>
        /// <param name="changeReason">Lý do thay đổi (chi tiết)</param>
        /// <param name="changeType">Loại thay đổi: AUTO_DEGRADE, MANUAL_ADJUST, REFUND, INITIAL</param>
        /// <param name="changedBy">User ID người thực hiện thay đổi</param>
        /// <param name="ipAddress">IP address của request (để security tracking)</param>
        /// <param name="userAgent">User Agent của request</param>
        /// <param name="refundAmount">Số tiền refund nếu có</param>
        /// <param name="usageDeducted">Có trừ lượt subscription không (cho MANUAL_ADJUST từ Extra → Subscription)</param>
        Task<ServiceSourceAuditLog> LogServiceSourceChangeAsync(
            int appointmentServiceId,
            string? oldServiceSource,
            string newServiceSource,
            decimal oldPrice,
            decimal newPrice,
            string changeReason,
            string changeType,
            int changedBy,
            string? ipAddress = null,
            string? userAgent = null,
            decimal? refundAmount = null,
            bool usageDeducted = false);

        /// <summary>
        /// Lấy danh sách audit logs cho một Appointment
        /// </summary>
        /// <param name="appointmentId">ID của Appointment</param>
        /// <returns>Danh sách audit logs sắp xếp theo thời gian</returns>
        Task<List<object>> GetAuditLogsForAppointmentAsync(int appointmentId);

        /// <summary>
        /// Lấy audit logs cho một AppointmentService cụ thể
        /// </summary>
        /// <param name="appointmentServiceId">ID của AppointmentService</param>
        /// <returns>Danh sách audit logs của service đó</returns>
        Task<List<ServiceSourceAuditLog>> GetAuditLogsForServiceAsync(int appointmentServiceId);
    }
}
