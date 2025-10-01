using EVServiceCenter.Core.Domains.CarModels.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarModels.DTOs.Responses;
using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.CarModels.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarModels.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Application.Domains.CarModels.Services
{
    public class CarModelQueryService : ICarModelQueryService
    {
        private readonly ICarModelRepository _repository;
        private readonly ICarModelStatisticsRepository _statsRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CarModelQueryService> _logger;

        public CarModelQueryService(
            ICarModelRepository repository,
            ICarModelStatisticsRepository statsRepository,
            IMemoryCache cache,
            ILogger<CarModelQueryService> logger)
        {
            _repository = repository;
            _statsRepository = statsRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PagedResult<CarModelResponseDto>> GetAllAsync(
            CarModelQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var modelsQuery = _repository.GetQueryable();

            // Include Brand if requested
            if (query.IncludeBrand)
                modelsQuery = modelsQuery.Include(m => m.Brand);

            // Apply filters
            modelsQuery = ApplyFilters(modelsQuery, query);

            // Get total
            var totalCount = await modelsQuery.CountAsync(cancellationToken);

            // Apply sorting & pagination
            modelsQuery = ApplySorting(modelsQuery, query.SortBy, query.SortOrder);
            var models = await modelsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map with optional stats
            var dtos = await MapToDtosAsync(models, query.IncludeStats, cancellationToken);

            return PagedResultFactory.Create(dtos, totalCount, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<CarModelResponseDto>> GetActiveModelsAsync(
            CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue<List<CarModelResponseDto>>("CarModels_Active", out var cached))
                return cached;

            var models = await _repository.GetQueryable()
                .Include(m => m.Brand)
                .Where(m => m.IsActive == true)
                .OrderBy(m => m.Brand.BrandName)
                .ThenBy(m => m.ModelName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = models.Select(MapToDto).ToList();
            _cache.Set("CarModels_Active", dtos, TimeSpan.FromMinutes(5));

            return dtos;
        }

        public async Task<IEnumerable<CarModelResponseDto>> GetModelsByBrandAsync(
            int brandId,
            CancellationToken cancellationToken = default)
        {
            var models = await _repository.GetQueryable()
                .Include(m => m.Brand)
                .Where(m => m.BrandId == brandId)
                .OrderBy(m => m.ModelName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return models.Select(MapToDto);
        }

        public async Task<IEnumerable<CarModelResponseDto>> SearchModelsAsync(
            string searchTerm,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveModelsAsync(cancellationToken);

            var lower = searchTerm.ToLower();
            var models = await _repository.GetQueryable()
                .Include(m => m.Brand)
                .Where(m => m.ModelName.ToLower().Contains(lower) ||
                           m.Brand.BrandName.ToLower().Contains(lower))
                .OrderBy(m => m.Brand.BrandName)
                .ThenBy(m => m.ModelName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return models.Select(MapToDto);
        }

        private async Task<List<CarModelResponseDto>> MapToDtosAsync(
            List<CarModel> models,
            bool includeStats,
            CancellationToken cancellationToken)
        {
            var dtos = models.Select(MapToDto).ToList();

            if (includeStats && models.Any())
            {
                var modelIds = models.Select(m => m.ModelId).ToList();
                var stats = await _statsRepository.GetBatchStatisticsAsync(modelIds, cancellationToken);

                foreach (var dto in dtos)
                {
                    if (stats.TryGetValue(dto.ModelId, out var modelStats))
                    {
                        dto.TotalVehicles = modelStats.TotalVehicles;
                        dto.ActiveVehicles = modelStats.ActiveVehicles;
                        dto.TotalServicesPerformed = modelStats.TotalServicesPerformed;
                    }
                }
            }

            return dtos;
        }

        private CarModelResponseDto MapToDto(CarModel model)
        {
            return new CarModelResponseDto
            {
                ModelId = model.ModelId,
                BrandId = model.BrandId,
                BrandName = model.Brand?.BrandName ?? string.Empty,
                ModelName = model.ModelName,
                FullModelName = $"{model.Brand?.BrandName} {model.ModelName}",
                Year = model.Year,
                BatteryCapacity = model.BatteryCapacity,
                MaxRange = model.MaxRange,
                ChargingType = model.ChargingType,
                MotorPower = model.MotorPower,
                AccelerationTime = model.AccelerationTime,
                TopSpeed = model.TopSpeed,
                ServiceInterval = model.ServiceInterval,
                ServiceIntervalMonths = model.ServiceIntervalMonths,
                WarrantyPeriod = model.WarrantyPeriod,
                ImageUrl = model.ImageUrl,
                Description = model.Description,
                IsActive = model.IsActive ?? false,
                CreatedDate = model.CreatedDate ?? DateTime.UtcNow,
                UpdatedDate = model.UpdatedDate
            };
        }

        private static IQueryable<CarModel> ApplyFilters(
            IQueryable<CarModel> query,
            CarModelQueryDto filters)
        {
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var search = filters.SearchTerm.ToLower();
                query = query.Where(m =>
                    m.ModelName.ToLower().Contains(search) ||
                    m.Brand.BrandName.ToLower().Contains(search));
            }

            if (filters.BrandId.HasValue)
                query = query.Where(m => m.BrandId == filters.BrandId.Value);

            if (filters.Year.HasValue)
                query = query.Where(m => m.Year == filters.Year.Value);

            if (filters.IsActive.HasValue)
                query = query.Where(m => m.IsActive == filters.IsActive.Value);

            return query;
        }

        private static IQueryable<CarModel> ApplySorting(
            IQueryable<CarModel> query,
            string sortBy,
            string sortOrder)
        {
            var isDesc = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "modelname" => isDesc ? query.OrderByDescending(m => m.ModelName) : query.OrderBy(m => m.ModelName),
                "brandname" => isDesc ? query.OrderByDescending(m => m.Brand.BrandName) : query.OrderBy(m => m.Brand.BrandName),
                "year" => isDesc ? query.OrderByDescending(m => m.Year) : query.OrderBy(m => m.Year),
                "batterycapacity" => isDesc ? query.OrderByDescending(m => m.BatteryCapacity) : query.OrderBy(m => m.BatteryCapacity),
                "maxrange" => isDesc ? query.OrderByDescending(m => m.MaxRange) : query.OrderBy(m => m.MaxRange),
                "createddate" => isDesc ? query.OrderByDescending(m => m.CreatedDate) : query.OrderBy(m => m.CreatedDate),
                "isactive" => isDesc ? query.OrderByDescending(m => m.IsActive) : query.OrderBy(m => m.IsActive),
                _ => query.OrderBy(m => m.Brand.BrandName).ThenBy(m => m.ModelName)
            };
        }
    }
}