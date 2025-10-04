using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.MaintenanceServices.Repositories
{
    public class MaintenanceServiceRepository : Repository<MaintenanceService>, IMaintenanceServiceRepository
    {
        public MaintenanceServiceRepository(EVDbContext context) : base(context) { }

        public IQueryable<MaintenanceService> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<IEnumerable<MaintenanceService>> GetByIdsAsync(
            List<int> serviceIds,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => serviceIds.Contains(s.ServiceId))
                .Include(s => s.Category)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<MaintenanceService?> GetByIdWithDetailsAsync(
            int serviceId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(s => s.Category)
                .Include(s => s.ModelServicePricings)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ServiceId == serviceId, cancellationToken);
        }

        public async Task<bool> IsServiceCodeExistsAsync(
            string serviceCode,
            int? excludeServiceId = null,
            CancellationToken cancellationToken = default)
        {
            var normalizedCode = serviceCode.ToUpper();
            var query = _dbSet.Where(s => s.ServiceCode.ToUpper() == normalizedCode);

            if (excludeServiceId.HasValue)
                query = query.Where(s => s.ServiceId != excludeServiceId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceService>> GetServicesByCategoryAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.CategoryId == categoryId)
                .Include(s => s.Category)
                .OrderBy(s => s.ServiceName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<MaintenanceService>> GetActiveServicesAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(s => s.IsActive == true)
                .Include(s => s.Category)
                .OrderBy(s => s.Category.DisplayOrder)
                .ThenBy(s => s.ServiceName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> CanDeleteAsync(
            int serviceId,
            CancellationToken cancellationToken = default)
        {
            // Check if service has any appointments
            var hasAppointments = await _context.Appointments
                .AnyAsync(a => a.ServiceId == serviceId, cancellationToken);

            if (hasAppointments) return false;

            // Check if service has any work orders
            var hasWorkOrders = await _context.WorkOrderServices
                .AnyAsync(wo => wo.ServiceId == serviceId, cancellationToken);

            if (hasWorkOrders) return false;

            // Check if service is in any packages
            var hasPackages = await _context.PackageServices
                .AnyAsync(ps => ps.ServiceId == serviceId, cancellationToken);

            if (hasPackages) return false;

            return true;
        }
    }
}