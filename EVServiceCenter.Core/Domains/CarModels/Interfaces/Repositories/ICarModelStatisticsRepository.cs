using EVServiceCenter.Core.Domains.CarModels.DTOs;

namespace EVServiceCenter.Core.Domains.CarModels.Interfaces.Repositories
{
    public interface ICarModelStatisticsRepository
    {
        Task<Dictionary<int, ModelStatistics>> GetBatchStatisticsAsync(
            IEnumerable<int> modelIds,
            CancellationToken cancellationToken = default);
    }
}