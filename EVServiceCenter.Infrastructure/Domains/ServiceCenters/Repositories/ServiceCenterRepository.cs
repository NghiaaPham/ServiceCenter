using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.ServiceCenters.Repositories
{
    public class ServiceCenterRepository : Repository<ServiceCenter>, IServiceCenterRepository
    {
        public ServiceCenterRepository(EVDbContext context) : base(context) { }

        public IQueryable<ServiceCenter> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }
        public async Task<ServiceCenter?> GetByCenterCodeAsync(
            string centerCode,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(centerCode))
                return null;

            return await _dbSet
                .Include(c => c.Manager)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CenterCode == centerCode, cancellationToken);
        }

        public async Task<ServiceCenter?> GetByIdWithDetailsAsync(
            int centerId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(c => c.Manager)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CenterId == centerId, cancellationToken);
        }

            public async Task<bool> IsCenterCodeExistsAsync(
        string centerCode,
        int? excludeCenterId = null,
        CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(centerCode))
                return false;

            var query = _dbSet.Where(c => c.CenterCode == centerCode);

            // QUAN TRỌNG: Phải loại trừ center đang update
            if (excludeCenterId.HasValue)
                query = query.Where(c => c.CenterId != excludeCenterId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> CanDeleteAsync(
    int centerId,
    CancellationToken cancellationToken = default)
        {
            var hasRelatedData =
                await _context.Appointments.AnyAsync(a => a.ServiceCenterId == centerId, cancellationToken) ||
                await _context.WorkOrders.AnyAsync(w => w.ServiceCenterId == centerId, cancellationToken) ||
                await _context.Departments.AnyAsync(d => d.CenterId == centerId, cancellationToken);

            return !hasRelatedData;
        }

    }
}