using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services
{
    public interface IAppointmentQueryService
    {
        Task<PagedResult<AppointmentResponseDto>> GetPagedAsync(
            AppointmentQueryDto query,
            CancellationToken cancellationToken = default);

        Task<AppointmentDetailResponseDto?> GetByIdAsync(
            int appointmentId,
            CancellationToken cancellationToken = default);

        Task<AppointmentResponseDto?> GetByCodeAsync(
            string appointmentCode,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<AppointmentResponseDto>> GetByCustomerIdAsync(
            int customerId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<AppointmentResponseDto>> GetUpcomingByCustomerAsync(
            int customerId,
            int limit = 5,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<AppointmentResponseDto>> GetByServiceCenterAndDateAsync(
            int serviceCenterId,
            DateOnly date,
            CancellationToken cancellationToken = default);

        Task<int> GetCountByStatusAsync(
            int statusId,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<PaymentIntentResponseDto>> GetPaymentIntentsAsync(
            int appointmentId,
            CancellationToken cancellationToken = default);
    }
}
