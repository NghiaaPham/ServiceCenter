namespace EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services
{
    /// <summary>
    /// ✅ FIX GAP #14: Email confirmation service for appointment actions
    /// Service to send email confirmations for appointment lifecycle events
    /// </summary>
    public interface IAppointmentEmailService
    {
        /// <summary>
        /// Send appointment confirmation email
        /// </summary>
        Task SendAppointmentConfirmationAsync(
            int appointmentId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send appointment cancellation email
        /// </summary>
        Task SendAppointmentCancellationAsync(
            int appointmentId,
            string cancellationReason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send appointment reminder email (24h before)
        /// </summary>
        Task SendAppointmentReminderAsync(
            int appointmentId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send appointment completion email with invoice
        /// </summary>
        Task SendAppointmentCompletionAsync(
            int appointmentId,
            int? invoiceId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Send reschedule confirmation email
        /// </summary>
        Task SendRescheduleConfirmationAsync(
            int oldAppointmentId,
            int newAppointmentId,
            CancellationToken cancellationToken = default);
    }
}
