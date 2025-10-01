using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.Shared.Interfaces;

namespace EVServiceCenter.Core.Domains.CarModels.Interfaces.Repositories
{
    public interface ICarModelRepository : IRepository<CarModel>
    {
        IQueryable<CarModel> GetQueryable();

        Task<CarModel?> GetByIdWithBrandAsync(
            int modelId,
            CancellationToken cancellationToken = default);

        Task<bool> IsModelNameExistsAsync(
            int brandId,
            string modelName,
            int? excludeModelId = null,
            CancellationToken cancellationToken = default);

        Task<bool> CanDeleteAsync(
            int modelId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CarModel>> GetModelsByBrandAsync(
            int brandId,
            CancellationToken cancellationToken = default);
    }
}