using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Query;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response;


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
        /// NEW: Optimized projection returning response DTOs for upcoming appointments (single-query)
        /// </summary>
        Task<IEnumerable<AppointmentResponseDto>> GetUpcomingDtosByCustomerAsync(
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
        /// <summary>
        /// Đếm số lần appointment gốc đã được reschedule
        /// </summary>
        Task<int> CountRescheduleTimesAsync(
            int appointmentId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra appointment có đang bị reschedule không (có appointment mới point về nó)
        /// </summary>
        Task<bool> HasBeenRescheduledAsync(
            int appointmentId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy toàn bộ reschedule chain (từ gốc đến mới nhất)
        /// Trả về danh sách AppointmentId theo thứ tự: [Original] -> [Reschedule 1] -> [Reschedule 2]
        /// </summary>
        Task<List<int>> GetRescheduleChainAsync(
            int appointmentId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy tất cả appointments của vehicle trong một ngày cụ thể
        /// Dùng để kiểm tra vehicle time conflict
        /// </summary>
        Task<List<Appointment>> GetVehicleAppointmentsByDateAsync(
            int vehicleId,
            DateOnly date,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy tất cả appointments của technician trong một ngày cụ thể (PER CENTER)
        /// Dùng để kiểm tra technician time conflict
        /// </summary>
        Task<List<Appointment>> GetTechnicianAppointmentsByDateAsync(
            int technicianId,
            int serviceCenterId,
            DateOnly date,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy tất cả appointments của service center trong một ngày cụ thể
        /// Dùng để kiểm tra service center capacity conflict
        /// </summary>
        Task<List<Appointment>> GetServiceCenterAppointmentsByDateAsync(
            int serviceCenterId,
            DateOnly date,
            CancellationToken cancellationToken = default);
    }
}
