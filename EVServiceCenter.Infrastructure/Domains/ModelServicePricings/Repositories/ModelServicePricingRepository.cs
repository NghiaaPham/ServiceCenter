using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.ModelServicePricings.Repositories
{
    public class ModelServicePricingRepository : Repository<ModelServicePricing>, IModelServicePricingRepository
    {
        public ModelServicePricingRepository(EVDbContext context) : base(context) { }

        public IQueryable<ModelServicePricing> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<ModelServicePricing?> GetByIdWithDetailsAsync(
            int pricingId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(p => p.Model)
                    .ThenInclude(m => m.Brand)
                .Include(p => p.Service)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PricingId == pricingId, cancellationToken);
        }

        public async Task<bool> IsDuplicateAsync(
            int modelId,
            int serviceId,
            int? excludePricingId = null,
            CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(p => p.ModelId == modelId && p.ServiceId == serviceId);

            if (excludePricingId.HasValue)
                query = query.Where(p => p.PricingId != excludePricingId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<IEnumerable<ModelServicePricing>> GetByModelIdAsync(
            int modelId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.ModelId == modelId)
                .Include(p => p.Service)
                .OrderBy(p => p.Service.ServiceName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ModelServicePricing>> GetByServiceIdAsync(
            int serviceId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.ServiceId == serviceId)
                .Include(p => p.Model)
                    .ThenInclude(m => m.Brand)
                .OrderBy(p => p.Model.Brand.BrandName)
                    .ThenBy(p => p.Model.ModelName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<ModelServicePricing?> GetActivePricingAsync(
            int modelId,
            int serviceId,
            DateOnly? forDate = null,
            CancellationToken cancellationToken = default)
        {
            var checkDate = forDate ?? DateOnly.FromDateTime(DateTime.Today);

            return await _dbSet
                .Where(p => p.ModelId == modelId
                    && p.ServiceId == serviceId
                    && p.IsActive == true
                    && (!p.EffectiveDate.HasValue || p.EffectiveDate <= checkDate)
                    && (!p.ExpiryDate.HasValue || p.ExpiryDate >= checkDate))
                .OrderByDescending(p => p.EffectiveDate)
                .Include(p => p.Service)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}