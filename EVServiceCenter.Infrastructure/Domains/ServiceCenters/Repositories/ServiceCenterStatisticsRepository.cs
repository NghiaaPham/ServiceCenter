using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.ServiceCenters.Repositories
{
    public class ServiceCenterStatisticsRepository : IServiceCenterStatisticsRepository
    {
        private readonly EVDbContext _context;

        public ServiceCenterStatisticsRepository(EVDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Dictionary<int, int>> GetAppointmentCountsAsync(
            IEnumerable<int> centerIds,
            CancellationToken cancellationToken = default)
        {
            var ids = centerIds.ToList();
            if (!ids.Any())
                return new Dictionary<int, int>();

            return await _context.Appointments
                .Where(a => ids.Contains(a.ServiceCenterId))
                .GroupBy(a => a.ServiceCenterId)
                .Select(g => new { CenterId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CenterId, x => x.Count, cancellationToken);
        }

        public async Task<Dictionary<int, int>> GetWorkOrderCountsAsync(
            IEnumerable<int> centerIds,
            CancellationToken cancellationToken = default)
        {
            var ids = centerIds.ToList();
            if (!ids.Any())
                return new Dictionary<int, int>();

            return await _context.WorkOrders
                .Where(w => ids.Contains(w.ServiceCenterId))
                .GroupBy(w => w.ServiceCenterId)
                .Select(g => new { CenterId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.CenterId, x => x.Count, cancellationToken);
        }

        public async Task<Dictionary<int, int>> GetTechnicianCountsAsync(
            IEnumerable<int> centerIds,
            CancellationToken cancellationToken = default)
        {
            var ids = centerIds.ToList();
            if (!ids.Any())
                return new Dictionary<int, int>();

            return await _context.TechnicianSchedules
                .Where(ts => ids.Contains(ts.CenterId))
                .GroupBy(ts => ts.CenterId)
                .Select(g => new { CenterId = g.Key, Count = g.Select(x => x.TechnicianId).Distinct().Count() })
                .ToDictionaryAsync(x => x.CenterId, x => x.Count, cancellationToken);
        }

        // TODO: Implement when Department module is ready
        public async Task<Dictionary<int, int>> GetDepartmentCountsAsync(
            IEnumerable<int> centerIds,
            CancellationToken cancellationToken = default)
        {
            // TEMPORARY: Return empty dictionary until Department module is implemented
            await Task.CompletedTask;
            return new Dictionary<int, int>();

            /* UNCOMMENT WHEN DEPARTMENT MODULE READY
            var ids = centerIds.ToList();
            if (!ids.Any())
                return new Dictionary<int, int>();

            return await _context.Departments
                .Where(d => d.CenterId.HasValue && ids.Contains(d.CenterId.Value) && d.IsActive)
                .GroupBy(d => d.CenterId)
                .Select(g => new { CenterId = g.Key!.Value, Count = g.Count() })
                .ToDictionaryAsync(x => x.CenterId, x => x.Count, cancellationToken);
            */
        }

        public async Task<Dictionary<int, decimal>> GetAverageRatingsAsync(
            IEnumerable<int> centerIds,
            CancellationToken cancellationToken = default)
        {
            var ids = centerIds.ToList();
            if (!ids.Any())
                return new Dictionary<int, decimal>();

            return await _context.ServiceRatings
                .Join(_context.WorkOrders,
                    sr => sr.WorkOrderId,
                    wo => wo.WorkOrderId,
                    (sr, wo) => new { sr.OverallRating, wo.ServiceCenterId })
                .Where(x => ids.Contains(x.ServiceCenterId))
                .GroupBy(x => x.ServiceCenterId)
                .Select(g => new { CenterId = g.Key, AvgRating = g.Average(x => (decimal)x.OverallRating) })
                .ToDictionaryAsync(x => x.CenterId, x => x.AvgRating, cancellationToken);
        }

        public async Task<Dictionary<int, decimal>> GetMonthlyRevenuesAsync(
            IEnumerable<int> centerIds,
            int year,
            int month,
            CancellationToken cancellationToken = default)
        {
            var ids = centerIds.ToList();
            if (!ids.Any())
                return new Dictionary<int, decimal>();

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            return await _context.Invoices
                .Join(_context.WorkOrders,
                    inv => inv.WorkOrderId,
                    wo => wo.WorkOrderId,
                    (inv, wo) => new { inv, wo.ServiceCenterId })
                .Where(x =>
                    ids.Contains(x.ServiceCenterId) &&
                    x.inv.InvoiceDate >= startDate &&
                    x.inv.InvoiceDate < endDate &&
                    x.inv.Status == "Paid")
                .GroupBy(x => x.ServiceCenterId)
                .Select(g => new { CenterId = g.Key, Revenue = g.Sum(x => x.inv.GrandTotal) })
                .ToDictionaryAsync(x => x.CenterId, x => x.Revenue ?? 0m, cancellationToken);
        }

        // TODO: Implement when TimeSlot module is ready
        public async Task<int> GetActiveTimeSlotsCountAsync(
            int centerId,
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            // TEMPORARY: Return 0 until TimeSlot module is implemented
            await Task.CompletedTask;
            return 0;

            /* UNCOMMENT WHEN TIMESLOT MODULE READY
            var dateOnly = DateOnly.FromDateTime(date);

            return await _context.TimeSlots
                .Where(ts => 
                    ts.CenterId == centerId &&
                    ts.SlotDate == dateOnly &&
                    ts.IsAvailable)
                .CountAsync(cancellationToken);
            */
        }
    }
}