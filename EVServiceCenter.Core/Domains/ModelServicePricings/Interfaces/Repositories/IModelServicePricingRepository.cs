using EVServiceCenter.Core.Domains.Shared.Interfaces;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Repositories
{
    public interface IModelServicePricingRepository : IRepository<ModelServicePricing>
    {
        Task<ModelServicePricing?> GetByIdWithDetailsAsync(int pricingId, CancellationToken cancellationToken = default);
        Task<bool> IsDuplicateAsync(int modelId, int serviceId, int? excludePricingId = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<ModelServicePricing>> GetByModelIdAsync(int modelId, CancellationToken cancellationToken = default);
        Task<IEnumerable<ModelServicePricing>> GetByServiceIdAsync(int serviceId, CancellationToken cancellationToken = default);
        Task<ModelServicePricing?> GetActivePricingAsync(int modelId, int serviceId, DateOnly? forDate = null, CancellationToken cancellationToken = default);
        IQueryable<ModelServicePricing> GetQueryable();
    }
}