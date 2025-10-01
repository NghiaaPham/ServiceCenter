namespace EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories
{
    public interface IServiceCenterStatisticsRepository
    {
        Task<Dictionary<int, int>> GetAppointmentCountsAsync(
            IEnumerable<int> centerIds,
            CancellationToken cancellationToken = default);

        Task<Dictionary<int, int>> GetWorkOrderCountsAsync(
            IEnumerable<int> centerIds,
            CancellationToken cancellationToken = default);

        Task<Dictionary<int, int>> GetTechnicianCountsAsync(
            IEnumerable<int> centerIds,
            CancellationToken cancellationToken = default);

        Task<Dictionary<int, int>> GetDepartmentCountsAsync(
            IEnumerable<int> centerIds,
            CancellationToken cancellationToken = default);

        Task<Dictionary<int, decimal>> GetAverageRatingsAsync(
            IEnumerable<int> centerIds,
            CancellationToken cancellationToken = default);

        Task<Dictionary<int, decimal>> GetMonthlyRevenuesAsync(
            IEnumerable<int> centerIds,
            int year,
            int month,
            CancellationToken cancellationToken = default);

        // Time slot queries
        Task<int> GetActiveTimeSlotsCountAsync(
            int centerId,
            DateTime date,
            CancellationToken cancellationToken = default);
    }
}