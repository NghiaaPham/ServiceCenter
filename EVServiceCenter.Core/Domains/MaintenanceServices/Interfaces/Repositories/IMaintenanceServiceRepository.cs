using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.Shared.Interfaces;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Repositories
{
    public interface IMaintenanceServiceRepository : IRepository<MaintenanceService>
    {
        Task<IEnumerable<MaintenanceService>> GetByIdsAsync(
            List<int> serviceIds,
            CancellationToken cancellationToken = default);
        Task<MaintenanceService?> GetByIdWithDetailsAsync(int serviceId, CancellationToken cancellationToken = default);
        Task<bool> IsServiceCodeExistsAsync(string serviceCode, int? excludeServiceId = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceService>> GetServicesByCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MaintenanceService>> GetActiveServicesAsync(CancellationToken cancellationToken = default);
        Task<bool> CanDeleteAsync(int serviceId, CancellationToken cancellationToken = default);
        IQueryable<MaintenanceService> GetQueryable();
    }
}