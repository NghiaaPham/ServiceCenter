using EVServiceCenter.Core.Domains.CarModels.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarModels.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Application.Domains.CarModels.Services
{
    public class CarModelStatisticsService : ICarModelStatisticsService
    {
        private readonly ICarModelRepository _repository;
        private readonly ICarModelStatisticsRepository _statsRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CarModelStatisticsService> _logger;

        public CarModelStatisticsService(
            ICarModelRepository repository,
            ICarModelStatisticsRepository statsRepository,
            IMemoryCache cache,
            ILogger<CarModelStatisticsService> logger)
        {
            _repository = repository;
            _statsRepository = statsRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<object> GetModelStatisticsAsync(
            int modelId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"CarModelStats_{modelId}";
            if (_cache.TryGetValue<object>(cacheKey, out var cached))
                return cached;

            var model = await _repository.GetByIdWithBrandAsync(modelId, cancellationToken);
            if (model == null)
                throw new InvalidOperationException($"Không tìm thấy dòng xe {modelId}");

            var modelStats = await _statsRepository.GetBatchStatisticsAsync(
                new[] { modelId },
                cancellationToken);

            var statsData = modelStats.GetValueOrDefault(modelId, new Core.Domains.CarModels.DTOs.ModelStatistics());

            var stats = new
            {
                ModelId = model.ModelId,
                BrandName = model.Brand?.BrandName,
                ModelName = model.ModelName,
                FullModelName = $"{model.Brand?.BrandName} {model.ModelName}",
                Year = model.Year,
                TotalVehicles = statsData.TotalVehicles,
                ActiveVehicles = statsData.ActiveVehicles,
                InactiveVehicles = statsData.TotalVehicles - statsData.ActiveVehicles,
                TotalServicesPerformed = statsData.TotalServicesPerformed,
                AverageServicesPerVehicle = statsData.TotalVehicles > 0
                    ? Math.Round((decimal)statsData.TotalServicesPerformed / statsData.TotalVehicles, 2)
                    : 0,
                IsActive = model.IsActive ?? false,
                LastUpdated = DateTime.UtcNow
            };

            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));

            return stats;
        }

        public async Task<object> GetAllModelsStatisticsAsync(
            CancellationToken cancellationToken = default)
        {
            var cacheKey = "CarModels_AllStats";
            if (_cache.TryGetValue<object>(cacheKey, out var cached))
                return cached;

            var models = await _repository.GetQueryable()
                .Include(m => m.Brand)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var stats = new
            {
                TotalModels = models.Count,
                ActiveModels = models.Count(m => m.IsActive == true),
                InactiveModels = models.Count(m => m.IsActive == false),
                ModelsByBrand = models
                    .GroupBy(m => new { m.BrandId, BrandName = m.Brand.BrandName })
                    .Select(g => new
                    {
                        BrandId = g.Key.BrandId,
                        BrandName = g.Key.BrandName,
                        TotalModels = g.Count(),
                        ActiveModels = g.Count(m => m.IsActive == true)
                    })
                    .OrderByDescending(x => x.TotalModels)
                    .ToList(),
                ModelsByYear = models
                    .Where(m => m.Year.HasValue)
                    .GroupBy(m => m.Year)
                    .Select(g => new
                    {
                        Year = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Year)
                    .ToList(),
                LastUpdated = DateTime.UtcNow
            };

            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));

            return stats;
        }

        public async Task<object> GetBrandModelsStatisticsAsync(
            int brandId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"BrandModels_Stats_{brandId}";
            if (_cache.TryGetValue<object>(cacheKey, out var cached))
                return cached;

            var models = await _repository.GetQueryable()
                .Include(m => m.Brand)
                .Where(m => m.BrandId == brandId)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (!models.Any())
                throw new InvalidOperationException($"Không tìm thấy dòng xe nào của thương hiệu {brandId}");

            var modelIds = models.Select(m => m.ModelId).ToList();
            var vehicleStats = await _statsRepository.GetBatchStatisticsAsync(modelIds, cancellationToken);

            var stats = new
            {
                BrandId = brandId,
                BrandName = models.First().Brand?.BrandName,
                TotalModels = models.Count,
                ActiveModels = models.Count(m => m.IsActive == true),
                InactiveModels = models.Count(m => m.IsActive == false),
                TotalVehiclesAcrossModels = vehicleStats.Sum(v => v.Value.TotalVehicles),
                TotalServicesAcrossModels = vehicleStats.Sum(v => v.Value.TotalServicesPerformed),
                ModelBreakdown = models.Select(m => new
                {
                    ModelId = m.ModelId,
                    ModelName = m.ModelName,
                    Year = m.Year,
                    IsActive = m.IsActive,
                    TotalVehicles = vehicleStats.GetValueOrDefault(m.ModelId)?.TotalVehicles ?? 0,
                    TotalServices = vehicleStats.GetValueOrDefault(m.ModelId)?.TotalServicesPerformed ?? 0
                })
                .OrderByDescending(x => x.TotalVehicles)
                .ToList(),
                LastUpdated = DateTime.UtcNow
            };

            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));

            return stats;
        }
    }
}