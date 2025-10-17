using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services
{
    /// <summary>
    /// ?? Appointment Metrics Service - Advanced analytics
    /// </summary>
    public interface IAppointmentMetricsService
    {
        /// <summary>
        /// ?? Payment Health Metrics
        /// Ph�n t�ch s?c kh?e thanh to�n (paid rate, outstanding, avg payment time)
        /// </summary>
        Task<PaymentHealthMetricsDto> GetPaymentHealthMetricsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? serviceCenterId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// ?? Subscription Usage Metrics
        /// Ph�n t�ch s? d?ng subscription (usage rate, savings, top packages)
        /// </summary>
        Task<SubscriptionUsageMetricsDto> GetSubscriptionUsageMetricsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? serviceCenterId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// ?? Degradation Metrics
        /// Ph�n t�ch service degradation (rate, revenue impact, reasons)
        /// </summary>
        Task<DegradationMetricsDto> GetDegradationMetricsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? serviceCenterId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// ?? Cancellation Metrics
        /// Ph�n t�ch h?y l?ch (cancel rate, notice time, reasons)
        /// </summary>
        Task<CancellationMetricsDto> GetCancellationMetricsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null,
            int? serviceCenterId = null,
            CancellationToken cancellationToken = default);
    }
}
