using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs;

namespace EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories
{
    public interface ICustomerVehicleStatisticsRepository
    {
        Task<Dictionary<int, VehicleStatistics>> GetBatchStatisticsAsync(
            IEnumerable<int> vehicleIds,
            CancellationToken cancellationToken = default);
    }
}