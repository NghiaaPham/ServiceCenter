using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Shared.Models;


namespace EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories
{
    public interface IAppointmentQueryRepository
    {
        /// <summary>
        /// Lấy danh sách appointment có phân trang và filter
        /// </summary>
        Task<PagedResult<Appointment>> GetPagedAsync(
            AppointmentQueryDto query,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy tất cả appointment của khách hàng
        /// </summary>
        Task<IEnumerable<Appointment>> GetByCustomerIdAsync(
            int customerId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy appointment theo trung tâm và ngày
        /// </summary>
        Task<IEnumerable<Appointment>> GetByServiceCenterAndDateAsync(
            int serviceCenterId,
            DateOnly date,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy các appointment sắp tới của khách hàng
        /// </summary>
        Task<IEnumerable<Appointment>> GetUpcomingByCustomerAsync(
            int customerId,
            int limit = 5,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Đếm số appointment theo status
        /// </summary>
        Task<int> GetCountByStatusAsync(
            int statusId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy các appointment trong cùng slot
        /// </summary>
        Task<IEnumerable<Appointment>> GetBySlotIdAsync(
            int slotId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Đếm số appointment active (pending, confirmed) trong slot
        /// </summary>
        Task<int> GetActiveCountBySlotIdAsync(
            int slotId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy appointment theo vehicle
        /// </summary>
        Task<IEnumerable<Appointment>> GetByVehicleIdAsync(
            int vehicleId,
            CancellationToken cancellationToken = default);
    }
}
