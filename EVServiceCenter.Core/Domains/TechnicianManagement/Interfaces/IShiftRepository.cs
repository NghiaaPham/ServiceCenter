using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Core.Domains.TechnicianManagement.Interfaces
{
    /// <summary>
    /// Repository interface for Shift data access
    /// </summary>
    public interface IShiftRepository
    {
        /// <summary>
        /// Get shift by ID
        /// </summary>
        Task<Shift?> GetByIdAsync(int shiftId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get shift for specific technician and date
        /// </summary>
        /// <param name="technicianId">Technician user ID</param>
        /// <param name="shiftDate">Date of shift</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Shift or null if not found</returns>
        Task<Shift?> GetByTechnicianAndDateAsync(
            int technicianId,
            DateOnly shiftDate,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get shifts for technician in date range
        /// </summary>
        Task<List<Shift>> GetShiftsByDateRangeAsync(
            int technicianId,
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Create new shift record
        /// </summary>
        Task<Shift> CreateAsync(Shift shift, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update existing shift
        /// </summary>
        Task<Shift> UpdateAsync(Shift shift, CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if technician has active (checked-in) shift
        /// </summary>
        /// <param name="technicianId">Technician user ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if has active shift</returns>
        Task<bool> HasActiveShiftAsync(
            int technicianId,
            CancellationToken cancellationToken = default);
    }
}
