using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Requests;
using EVServiceCenter.Core.Domains.TechnicianManagement.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces
{
    /// <summary>
    /// Service interface for technician shift/attendance management
    /// </summary>
    public interface IShiftService
    {
        /// <summary>
        /// Check-in technician for shift
        /// Creates or updates Shift record with CheckInTime
        /// </summary>
        /// <param name="technicianId">Technician user ID</param>
        /// <param name="request">Check-in details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Shift information with check-in time</returns>
        Task<ShiftResponseDto> CheckInAsync(
            int technicianId,
            CheckInRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check-out technician from shift
        /// Updates Shift record with CheckOutTime and calculates hours
        /// </summary>
        /// <param name="technicianId">Technician user ID</param>
        /// <param name="request">Check-out details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Completed shift information</returns>
        Task<ShiftResponseDto> CheckOutAsync(
            int technicianId,
            CheckOutRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if technician is currently on-shift
        /// Used for WorkOrder start validation
        /// </summary>
        /// <param name="technicianId">Technician user ID</param>
        /// <param name="dateTime">Time to check (default: now)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if technician is on-shift</returns>
        Task<bool> IsOnShiftAsync(
            int technicianId,
            DateTime? dateTime = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get today's shift for technician
        /// </summary>
        /// <param name="technicianId">Technician user ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Today's shift or null if not found</returns>
        Task<ShiftResponseDto?> GetTodayShiftAsync(
            int technicianId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get shift history for technician
        /// </summary>
        /// <param name="technicianId">Technician user ID</param>
        /// <param name="from">Start date</param>
        /// <param name="to">End date</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of shifts in date range</returns>
        Task<List<ShiftResponseDto>> GetShiftHistoryAsync(
            int technicianId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default);
    }
}
