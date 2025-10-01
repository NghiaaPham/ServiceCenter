using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.CarModels.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.CarModels.Repositories
{
    public class CarModelRepository : Repository<CarModel>, ICarModelRepository
    {
        public CarModelRepository(EVDbContext context) : base(context) { }

        public IQueryable<CarModel> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<CarModel?> GetByIdWithBrandAsync(
            int modelId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(m => m.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ModelId == modelId, cancellationToken);
        }

        public async Task<bool> IsModelNameExistsAsync(
            int brandId,
            string modelName,
            int? excludeModelId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(modelName))
                return false;

            var query = _dbSet.Where(m => m.BrandId == brandId && m.ModelName == modelName);

            if (excludeModelId.HasValue)
                query = query.Where(m => m.ModelId != excludeModelId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> CanDeleteAsync(
            int modelId,
            CancellationToken cancellationToken = default)
        {
            var hasVehicles = await _context.CustomerVehicles
                .AnyAsync(v => v.ModelId == modelId, cancellationToken);

            var hasPricing = await _context.Set<ModelServicePricing>()
                .AnyAsync(p => p.ModelId == modelId, cancellationToken);

            return !hasVehicles && !hasPricing;
        }

        public async Task<IEnumerable<CarModel>> GetModelsByBrandAsync(
            int brandId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(m => m.BrandId == brandId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}