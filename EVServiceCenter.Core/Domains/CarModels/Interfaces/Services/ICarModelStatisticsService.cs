namespace EVServiceCenter.Core.Domains.CarModels.Interfaces.Services
{
    public interface ICarModelStatisticsService
    {
        Task<object> GetModelStatisticsAsync(
            int modelId,
            CancellationToken cancellationToken = default);

        Task<object> GetAllModelsStatisticsAsync(
            CancellationToken cancellationToken = default);

        Task<object> GetBrandModelsStatisticsAsync(
            int brandId,
            CancellationToken cancellationToken = default);
    }
}