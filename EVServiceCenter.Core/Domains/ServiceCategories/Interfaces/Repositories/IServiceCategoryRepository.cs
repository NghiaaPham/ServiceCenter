using EVServiceCenter.Core.Domains.ServiceCategories.Entities;
using EVServiceCenter.Core.Domains.Shared.Interfaces;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Repositories
{
    public interface IServiceCategoryRepository : IRepository<ServiceCategory>
    {
        Task<ServiceCategory?> GetByIdWithDetailsAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<bool> IsCategoryNameExistsAsync(string categoryName, int? excludeCategoryId = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<ServiceCategory>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
        Task<bool> CanDeleteAsync(int categoryId, CancellationToken cancellationToken = default);
        IQueryable<ServiceCategory> GetQueryable();
    }
}