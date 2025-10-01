using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.CustomerVehicles.Repositories
{
    public class CustomerVehicleStatisticsRepository : ICustomerVehicleStatisticsRepository
    {
        private readonly EVDbContext _context;

        public CustomerVehicleStatisticsRepository(EVDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<int, VehicleStatistics>> GetBatchStatisticsAsync(
            IEnumerable<int> vehicleIds,
            CancellationToken cancellationToken = default)
        {
            var ids = vehicleIds.ToList();
            if (!ids.Any())
                return new Dictionary<int, VehicleStatistics>();

            // TODO: Implement when WorkOrder and MaintenanceHistory modules are ready
            // For now, return empty statistics
            var stats = ids.ToDictionary(
                id => id,
                id => new VehicleStatistics
                {
                    TotalWorkOrders = 0,
                    TotalMaintenanceRecords = 0,
                    TotalSpentOnVehicle = 0
                });

            // Uncomment when WorkOrder module is ready:
            /*
            var workOrderStats = await _context.WorkOrders
                .Where(w => ids.Contains(w.VehicleId))
                .GroupBy(w => w.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    TotalWorkOrders = g.Count(),
                    TotalSpent = g.Sum(w => w.TotalAmount ?? 0)
                })
                .ToDictionaryAsync(x => x.VehicleId, cancellationToken);

            var maintenanceStats = await _context.MaintenanceHistories
                .Where(m => ids.Contains(m.VehicleId))
                .GroupBy(m => m.VehicleId)
                .Select(g => new
                {
                    VehicleId = g.Key,
                    TotalRecords = g.Count()
                })
                .ToDictionaryAsync(x => x.VehicleId, cancellationToken);

            foreach (var id in ids)
            {
                stats[id] = new VehicleStatistics
                {
                    TotalWorkOrders = workOrderStats.GetValueOrDefault(id)?.TotalWorkOrders ?? 0,
                    TotalMaintenanceRecords = maintenanceStats.GetValueOrDefault(id)?.TotalRecords ?? 0,
                    TotalSpentOnVehicle = workOrderStats.GetValueOrDefault(id)?.TotalSpent ?? 0
                };
            }
            */

            return stats;
        }
    }
}