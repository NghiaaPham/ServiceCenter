using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;


namespace EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services
{
    public interface IAppointmentCommandService
    {
        Task<AppointmentResponseDto> CreateAsync(
            CreateAppointmentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<AppointmentResponseDto> UpdateAsync(
            UpdateAppointmentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<AppointmentResponseDto> RescheduleAsync(
            RescheduleAppointmentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<bool> CancelAsync(
            CancelAppointmentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<bool> ConfirmAsync(
            ConfirmAppointmentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check-in khách hàng khi đến trung tâm (Confirmed → InProgress)
        /// Tạo WorkOrder để tracking công việc
        /// </summary>
        Task<AppointmentResponseDto> CheckInAsync(
            int appointmentId,
            int currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Thêm dịch vụ phát sinh khi đang InProgress
        /// Tạo PaymentIntent mới cho các dịch vụ bổ sung
        /// </summary>
        Task<AppointmentResponseDto> AddServicesAsync(
            int appointmentId,
            List<int> additionalServiceIds,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<bool> MarkAsNoShowAsync(
            int appointmentId,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int appointmentId,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<AppointmentResponseDto> RecordPaymentResultAsync(
            RecordPaymentResultRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(
            CreatePaymentIntentRequestDto request,
            int currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Complete appointment và update subscription usage (nếu có)
        /// </summary>
        Task<bool> CompleteAppointmentAsync(
            int appointmentId,
            int currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// [ADMIN TOOL] Điều chỉnh ServiceSource của một AppointmentService
        /// Dùng để sửa lỗi hoặc hoàn tiền cho customer
        /// </summary>
        Task<AdjustServiceSourceResponseDto> AdjustServiceSourceAsync(
            int appointmentId,
            int appointmentServiceId,
            string newServiceSource,
            decimal newPrice,
            string reason,
            bool issueRefund,
            int userId,
            string? ipAddress = null,
            string? userAgent = null,
            CancellationToken cancellationToken = default);
    }
}
