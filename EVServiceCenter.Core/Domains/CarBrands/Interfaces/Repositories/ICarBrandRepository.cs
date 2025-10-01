using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.Shared.Interfaces;

namespace EVServiceCenter.Core.Domains.CarBrands.Interfaces.Repositories
{
    public interface ICarBrandRepository : IRepository<CarBrand>
    {
        IQueryable<CarBrand> GetQueryable();

        Task<CarBrand?> GetByNameAsync(
            string brandName,
            CancellationToken cancellationToken = default);

        Task<bool> IsBrandNameExistsAsync(
            string brandName,
            int? excludeBrandId = null,
            CancellationToken cancellationToken = default);

        Task<bool> CanDeleteAsync(
            int brandId,
            CancellationToken cancellationToken = default);
    }
}