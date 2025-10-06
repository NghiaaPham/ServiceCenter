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

        Task<bool> MarkAsNoShowAsync(
            int appointmentId,
            int currentUserId,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int appointmentId,
            int currentUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Complete appointment và update subscription usage (nếu có)
        /// </summary>
        Task<bool> CompleteAppointmentAsync(
            int appointmentId,
            int currentUserId,
            CancellationToken cancellationToken = default);
    }
}
