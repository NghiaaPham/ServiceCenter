using EVServiceCenter.Core.Domains.CarModels.DTOs;
using EVServiceCenter.Core.Domains.CarModels.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.CarModels.Repositories
{
    public class CarModelStatisticsRepository : ICarModelStatisticsRepository
    {
        private readonly EVDbContext _context;

        public CarModelStatisticsRepository(EVDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<int, ModelStatistics>> GetBatchStatisticsAsync(
    IEnumerable<int> modelIds,
    CancellationToken cancellationToken = default)
        {
            var ids = modelIds.ToList();
            if (!ids.Any())
                return new Dictionary<int, ModelStatistics>();

            var stats = await (
                from model in _context.CarModels
                where ids.Contains(model.ModelId)
                select new
                {
                    ModelId = model.ModelId,
                    TotalVehicles = model.CustomerVehicles.Count(),
                    ActiveVehicles = model.CustomerVehicles.Count(v => v.IsActive == true),
                    // Tổng số work orders (dịch vụ thực hiện)
                    TotalServicesPerformed = model.CustomerVehicles
                        .SelectMany(v => v.WorkOrders)
                        .Count(),
                    // Hoặc tổng maintenance histories
                    TotalMaintenances = model.CustomerVehicles
                        .SelectMany(v => v.MaintenanceHistories)
                        .Count()
                })
                .ToDictionaryAsync(
                    x => x.ModelId,
                    x => new ModelStatistics
                    {
                        TotalVehicles = x.TotalVehicles,
                        ActiveVehicles = x.ActiveVehicles,
                        TotalServicesPerformed = x.TotalServicesPerformed
                    },
                    cancellationToken);

            return stats;
        }
    }
}