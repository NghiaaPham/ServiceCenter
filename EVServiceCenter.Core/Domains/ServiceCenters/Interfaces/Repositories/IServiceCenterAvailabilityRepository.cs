namespace EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories
{
    /// <summary>
    /// Availability and capacity queries for ServiceCenter
    /// </summary>
    public interface IServiceCenterAvailabilityRepository
    {
        /// <summary>
        /// Gets daily booking counts for multiple centers
        /// Excludes cancelled and no-show appointments
        /// </summary>
        Task<Dictionary<int, int>> GetDailyBookingCountsAsync(
            IEnumerable<int> centerIds,
            DateTime date,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets daily booking count for single center
        /// </summary>
        Task<int> GetDailyBookingCountAsync(
            int centerId,
            DateTime date,
            CancellationToken cancellationToken = default);
    }
}