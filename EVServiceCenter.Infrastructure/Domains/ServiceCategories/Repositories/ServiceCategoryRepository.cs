using EVServiceCenter.Core.Domains.ServiceCategories.Entities;
using EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.ServiceCategories.Repositories
{
    public class ServiceCategoryRepository : Repository<ServiceCategory>, IServiceCategoryRepository
    {
        public ServiceCategoryRepository(EVDbContext context) : base(context) { }

        public IQueryable<ServiceCategory> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<ServiceCategory?> GetByIdWithDetailsAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.MaintenanceServices)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId, cancellationToken);
        }

        public async Task<bool> IsCategoryNameExistsAsync(
            string categoryName,
            int? excludeCategoryId = null,
            CancellationToken cancellationToken = default)
        {
            var normalizedName = categoryName.ToUpper();
            var query = _dbSet.Where(c => c.CategoryName.ToUpper() == normalizedName);

            if (excludeCategoryId.HasValue)
                query = query.Where(c => c.CategoryId != excludeCategoryId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<IEnumerable<ServiceCategory>> GetActiveCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> CanDeleteAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            var hasServices = await _context.MaintenanceServices
                .AnyAsync(s => s.CategoryId == categoryId, cancellationToken);

            return !hasServices;
        }
    }
}