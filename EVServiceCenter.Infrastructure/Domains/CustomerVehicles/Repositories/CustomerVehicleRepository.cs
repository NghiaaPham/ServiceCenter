using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.CustomerVehicles.Repositories
{
    public class CustomerVehicleRepository : Repository<CustomerVehicle>, ICustomerVehicleRepository
    {
        public CustomerVehicleRepository(EVDbContext context) : base(context) { }

        public IQueryable<CustomerVehicle> GetQueryable()
        {
            return _dbSet.AsQueryable();
        }

        public async Task<CustomerVehicle?> GetByIdWithDetailsAsync(
            int vehicleId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(v => v.Customer)
                .Include(v => v.Model)
                    .ThenInclude(m => m.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VehicleId == vehicleId, cancellationToken);
        }

        public async Task<CustomerVehicle?> GetByLicensePlateAsync(
            string licensePlate,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
                return null;

            return await _dbSet
                .Include(v => v.Customer)
                .Include(v => v.Model)
                    .ThenInclude(m => m.Brand)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.LicensePlate == licensePlate, cancellationToken);
        }

        public async Task<bool> IsLicensePlateExistsAsync(
            string licensePlate,
            int? excludeVehicleId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
                return false;

            var query = _dbSet.Where(v => v.LicensePlate == licensePlate);

            if (excludeVehicleId.HasValue)
                query = query.Where(v => v.VehicleId != excludeVehicleId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> IsVinExistsAsync(
            string vin,
            int? excludeVehicleId = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(vin))
                return false;

            var query = _dbSet.Where(v => v.Vin == vin);

            if (excludeVehicleId.HasValue)
                query = query.Where(v => v.VehicleId != excludeVehicleId.Value);

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> CanDeleteAsync(
            int vehicleId,
            CancellationToken cancellationToken = default)
        {
            // Check if vehicle has work orders
            var hasWorkOrders = await _context.Set<WorkOrder>()
                .AnyAsync(w => w.VehicleId == vehicleId, cancellationToken);

            // Check if vehicle has appointments
            var hasAppointments = await _context.Set<Appointment>()
                .AnyAsync(a => a.VehicleId == vehicleId, cancellationToken);

            return !hasWorkOrders && !hasAppointments;
        }

        public async Task<IEnumerable<CustomerVehicle>> GetVehiclesByCustomerAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(v => v.Customer) // ✅ ADDED: Include Customer for name & code
                .Include(v => v.Model)
                    .ThenInclude(m => m.Brand)
                .Where(v => v.CustomerId == customerId)
                .OrderByDescending(v => v.CreatedDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CustomerVehicle>> GetVehiclesByModelAsync(
            int modelId,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(v => v.Customer)
                .Where(v => v.ModelId == modelId)
                .OrderByDescending(v => v.CreatedDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<CustomerVehicle>> GetMaintenanceDueVehiclesAsync(
            CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var upcomingDays = 30;

            return await _dbSet
                .Include(v => v.Customer)
                .Include(v => v.Model)
                    .ThenInclude(m => m.Brand)
                .Where(v => v.IsActive == true &&
                    (v.NextMaintenanceDate.HasValue && v.NextMaintenanceDate <= today.AddDays(upcomingDays) ||
                     v.NextMaintenanceMileage.HasValue && v.Mileage.HasValue && v.Mileage >= v.NextMaintenanceMileage))
                .OrderBy(v => v.NextMaintenanceDate)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}