using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.ServiceCenters.Repositories
{
    public class ServiceCenterAvailabilityRepository : IServiceCenterAvailabilityRepository
    {
        private readonly EVDbContext _context;

        public ServiceCenterAvailabilityRepository(EVDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Dictionary<int, int>> GetDailyBookingCountsAsync(
    IEnumerable<int> centerIds,
    DateTime date,
    CancellationToken cancellationToken = default)
        {
            var ids = centerIds.ToList();
            if (!ids.Any())
                return new Dictionary<int, int>();

            var dateOnly = date.Date;
            var nextDay = dateOnly.AddDays(1);

            return await _context.Appointments
                .Where(a =>
                    ids.Contains(a.ServiceCenterId) &&
                    a.AppointmentDate >= dateOnly &&
                    a.AppointmentDate < nextDay &&
                    AppointmentStatusHelper.ActiveBookings.Contains(a.StatusId))  
                .GroupBy(a => a.ServiceCenterId)
                .Select(g => new { CenterId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CenterId, x => x.Count, cancellationToken);
        }

        public async Task<int> GetDailyBookingCountAsync(
            int centerId,
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            var dateOnly = date.Date;
            var nextDay = dateOnly.AddDays(1);

            return await _context.Appointments
                .Where(a =>
                    a.ServiceCenterId == centerId &&
                    a.AppointmentDate >= dateOnly &&
                    a.AppointmentDate < nextDay &&
                    AppointmentStatusHelper.ActiveBookings.Contains(a.StatusId))  
                .CountAsync(cancellationToken);
        }
    }
}