using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories
{
    public interface IAppointmentCommandRepository
    {
        Task<Appointment> CreateWithServicesAsync(
            Appointment appointment,
            List<AppointmentService> appointmentServices,
            CancellationToken cancellationToken = default);

        Task UpdateServicesAsync(
            int appointmentId,
            List<AppointmentService> newServices,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateStatusAsync(
            int appointmentId,
            int newStatusId,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateAsync(
            Appointment appointment,
            CancellationToken cancellationToken = default);

        Task<bool> CancelAsync(
            int appointmentId,
            string cancellationReason,
            CancellationToken cancellationToken = default);

        Task<Appointment> RescheduleAsync(
            int oldAppointmentId,
            Appointment newAppointment,
            List<AppointmentService> newServices,
            CancellationToken cancellationToken = default);

        Task<bool> ConfirmAsync(
            int appointmentId,
            string confirmationMethod,
            CancellationToken cancellationToken = default);

        Task<bool> MarkAsNoShowAsync(
            int appointmentId,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteIfPossibleAsync(
            int appointmentId,
            CancellationToken cancellationToken = default);
    }
}