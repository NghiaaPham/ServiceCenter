using EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVServiceCenter.Infrastructure.Domains.CarBrands.Repositories
{
    public class CarBrandStatisticsRepository : ICarBrandStatisticsRepository
    {
        private readonly EVDbContext _context;

        public CarBrandStatisticsRepository(EVDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<int, int>> GetModelCountsAsync(
            IEnumerable<int> brandIds,
            CancellationToken cancellationToken = default)
        {
            var ids = brandIds?.ToList();
            if (ids == null || ids.Count == 0)
                return new Dictionary<int, int>();

            return await _context.CarModels
                .AsNoTracking()
                .Where(m => ids.Contains(m.BrandId))
                .GroupBy(m => m.BrandId)
                .Select(g => new { BrandId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BrandId, x => x.Count, cancellationToken);
        }

        public async Task<Dictionary<int, int>> GetActiveModelCountsAsync(
            IEnumerable<int> brandIds,
            CancellationToken cancellationToken = default)
        {
            var ids = brandIds?.ToList();
            if (ids == null || ids.Count == 0)
                return new Dictionary<int, int>();

            return await _context.CarModels
                .AsNoTracking()
                .Where(m => ids.Contains(m.BrandId) && m.IsActive == true)
                .GroupBy(m => m.BrandId)
                .Select(g => new { BrandId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BrandId, x => x.Count, cancellationToken);
        }

        public async Task<Dictionary<int, int>> GetVehicleCountsAsync(
           IEnumerable<int> brandIds,
           CancellationToken cancellationToken = default)
        {
            var ids = brandIds.ToList();
            if (!ids.Any())
                return new Dictionary<int, int>();

            return await _context.CustomerVehicles
                .Join(_context.CarModels,
                    v => v.ModelId,
                    m => m.ModelId,
                    (v, m) => new { v, m.BrandId })
                .Where(x => ids.Contains(x.BrandId))
                .GroupBy(x => x.BrandId)
                .Select(g => new { BrandId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BrandId, x => x.Count, cancellationToken);
        }

        public async Task<Dictionary<int, BrandStatistics>> GetBatchStatisticsAsync(
     IEnumerable<int> brandIds,
     CancellationToken cancellationToken = default)
        {
            var ids = brandIds.ToList();
            if (!ids.Any())
                return new Dictionary<int, BrandStatistics>();

            // SINGLE QUERY với multiple aggregations
            var stats = await (
                from brand in _context.CarBrands
                where ids.Contains(brand.BrandId)
                select new
                {
                    BrandId = brand.BrandId,
                    TotalModels = brand.CarModels.Count(),
                    ActiveModels = brand.CarModels.Count(m => m.IsActive == true),
                    TotalVehicles = brand.CarModels
                        .SelectMany(m => m.CustomerVehicles)
                        .Count()
                })
                .ToDictionaryAsync(
                    x => x.BrandId,
                    x => new BrandStatistics
                    {
                        TotalModels = x.TotalModels,
                        ActiveModels = x.ActiveModels,
                        TotalVehicles = x.TotalVehicles
                    },
                    cancellationToken);

            return stats;
        }
    }
}
