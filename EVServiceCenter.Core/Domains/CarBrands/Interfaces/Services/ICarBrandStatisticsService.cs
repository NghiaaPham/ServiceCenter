namespace EVServiceCenter.Core.Domains.CarBrands.Interfaces.Services
{
    public interface ICarBrandStatisticsService
    {
        Task<object> GetBrandStatisticsAsync(
            int brandId,
            CancellationToken cancellationToken = default);

        Task<object> GetAllBrandsStatisticsAsync(
            CancellationToken cancellationToken = default);
    }
}