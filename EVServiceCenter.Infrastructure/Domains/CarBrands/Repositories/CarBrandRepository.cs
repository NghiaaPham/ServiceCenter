using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.CarBrands.Repositories
{
    public class CarBrandRepository : Repository<CarBrand>, ICarBrandRepository
    {
        public CarBrandRepository(EVDbContext context) : base(context) { }

        public IQueryable<CarBrand> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<CarBrand?> GetByNameAsync(
            string brandName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(brandName))
                return null;

            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BrandName == brandName, cancellationToken);
        }

        public async Task<bool> IsBrandNameExistsAsync(
            string brandName,
            int? excludeBrandId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(brandName))
                return false;

            var query = _dbSet.Where(b => b.BrandName.ToLower() == brandName.ToLower());

            if (excludeBrandId.HasValue)
                query = query.Where(b => b.BrandId != excludeBrandId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> CanDeleteAsync(
            int brandId,
            CancellationToken cancellationToken = default)
        {
            return !await _context.CarModels
                .AnyAsync(m => m.BrandId == brandId, cancellationToken);
        }
    }
}