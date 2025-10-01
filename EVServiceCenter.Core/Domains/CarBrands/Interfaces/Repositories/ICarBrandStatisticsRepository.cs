using EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests;

namespace EVServiceCenter.Core.Domains.CarBrands.Interfaces.Repositories
{
    public interface ICarBrandStatisticsRepository
    {
        Task<Dictionary<int, BrandStatistics>> GetBatchStatisticsAsync(
       IEnumerable<int> brandIds,
       CancellationToken cancellationToken = default);
        Task<Dictionary<int, int>> GetModelCountsAsync(
            IEnumerable<int> brandIds,
            CancellationToken cancellationToken = default);

        Task<Dictionary<int, int>> GetActiveModelCountsAsync(
            IEnumerable<int> brandIds,
            CancellationToken cancellationToken = default);

        Task<Dictionary<int, int>> GetVehicleCountsAsync(
            IEnumerable<int> brandIds,
            CancellationToken cancellationToken = default);
    }
}