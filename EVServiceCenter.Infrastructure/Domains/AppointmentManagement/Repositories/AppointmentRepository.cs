using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.ServiceCategories.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Infrastructure.Domains.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Repositories
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(EVDbContext context) : base(context)
        {
        }

        public async Task<Appointment?> GetByIdWithDetailsAsync(
    int appointmentId,
    CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Use Include to load CreatedByNavigation and UpdatedByNavigation
            return await _context.Appointments
                .AsNoTracking()
                .Include(a => a.Customer)
                .Include(a => a.Vehicle)
                    .ThenInclude(v => v.Model)
                        .ThenInclude(m => m!.Brand)
                .Include(a => a.ServiceCenter)
                .Include(a => a.Slot)
                .Include(a => a.Status)
                .Include(a => a.Package)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(aps => aps.Service)
                        .ThenInclude(s => s!.Category)
                .Include(a => a.PreferredTechnician)
                .Include(a => a.CreatedByNavigation) // ✅ ADDED: Load CreatedBy User
                .Include(a => a.UpdatedByNavigation) // ✅ ADDED: Load UpdatedBy User
                .Include(a => a.WorkOrders) // ✅ ADDED: Load WorkOrders
                    .ThenInclude(wo => wo.Status) // ✅ FIX: Load WorkOrder Status
                .Include(a => a.PaymentIntents)
                    .ThenInclude(pi => pi.PaymentTransactions)
                .AsSplitQuery() // ✅ PERFORMANCE: Prevent cartesian explosion
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId, cancellationToken);
        }

        public async Task<Appointment?> GetByCodeAsync(
            string appointmentCode,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AppointmentCode == appointmentCode, cancellationToken);
        }

        public async Task<bool> ExistsByCodeAsync(
            string appointmentCode,
            CancellationToken cancellationToken = default)
        {
            return await _context.Appointments
                .AsNoTracking()
                .AnyAsync(a => a.AppointmentCode == appointmentCode, cancellationToken);
        }
    }
}