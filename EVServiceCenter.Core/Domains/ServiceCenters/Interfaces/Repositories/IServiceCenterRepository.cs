using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.Shared.Interfaces;

namespace EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories
{
    /// <summary>
    /// Core CRUD operations for ServiceCenter entity
    /// </summary>
    public interface IServiceCenterRepository : IRepository<ServiceCenter>
    {
        // Query building
        IQueryable<ServiceCenter> GetQueryable();

        // Single entity queries
        Task<ServiceCenter?> GetByCenterCodeAsync(
            string centerCode,
            CancellationToken cancellationToken = default);

        Task<ServiceCenter?> GetByIdWithDetailsAsync(
            int centerId,
            CancellationToken cancellationToken = default);

        // Validation
        Task<bool> IsCenterCodeExistsAsync(
            string centerCode,
            int? excludeCenterId = null,
            CancellationToken cancellationToken = default);

        Task<bool> CanDeleteAsync(
            int centerId,
            CancellationToken cancellationToken = default);
    }
}