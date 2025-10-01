using EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Application.Domains.CarBrands.Services
{
    public class CarBrandStatisticsService : ICarBrandStatisticsService
    {
        private readonly ICarBrandRepository _repository;
        private readonly ICarBrandStatisticsRepository _statsRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CarBrandStatisticsService> _logger;

        public CarBrandStatisticsService(
            ICarBrandRepository repository,
            ICarBrandStatisticsRepository statsRepository,
            IMemoryCache cache,
            ILogger<CarBrandStatisticsService> logger)
        {
            _repository = repository;
            _statsRepository = statsRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<object> GetBrandStatisticsAsync(
    int brandId,
    CancellationToken cancellationToken = default)
        {
            var cacheKey = $"CarBrandStats_{brandId}";
            if (_cache.TryGetValue<object>(cacheKey, out var cached))
                return cached;

            var brand = await _repository.GetByIdAsync(brandId);
            if (brand == null)
                throw new InvalidOperationException($"Không tìm thấy thương hiệu {brandId}");

            
            var brandStats = await _statsRepository.GetBatchStatisticsAsync(
                new[] { brandId },
                cancellationToken);

            var statsData = brandStats.GetValueOrDefault(brandId, new BrandStatistics());

            var stats = new
            {
                BrandId = brand.BrandId,
                BrandName = brand.BrandName,
                Country = brand.Country,
                TotalModels = statsData.TotalModels,
                ActiveModels = statsData.ActiveModels,
                InactiveModels = statsData.TotalModels - statsData.ActiveModels,
                TotalVehicles = statsData.TotalVehicles,
                IsActive = brand.IsActive ?? false,
                LastUpdated = DateTime.UtcNow
            };

            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));

            return stats;
        }
        public async Task<object> GetAllBrandsStatisticsAsync(
            CancellationToken cancellationToken = default)
        {
            var cacheKey = "CarBrands_AllStats";
            if (_cache.TryGetValue<object>(cacheKey, out var cached))
                return cached;

            var brands = await _repository.GetQueryable()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var stats = new
            {
                TotalBrands = brands.Count,
                ActiveBrands = brands.Count(b => b.IsActive == true),
                InactiveBrands = brands.Count(b => b.IsActive == false),
                BrandsByCountry = brands
                    .GroupBy(b => b.Country ?? "Không xác định")
                    .Select(g => new
                    {
                        Country = g.Key,
                        Count = g.Count(),
                        ActiveCount = g.Count(b => b.IsActive == true)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                LastUpdated = DateTime.UtcNow
            };

            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));

            return stats;
        }
    }
}