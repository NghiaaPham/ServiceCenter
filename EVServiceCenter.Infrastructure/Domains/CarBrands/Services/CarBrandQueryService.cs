using EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarBrands.DTOs.Responses;
using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Application.Domains.CarBrands.Services
{
    public class CarBrandQueryService : ICarBrandQueryService
    {
        private readonly ICarBrandRepository _repository;
        private readonly ICarBrandStatisticsRepository _statsRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CarBrandQueryService> _logger;

        public CarBrandQueryService(
            ICarBrandRepository repository,
            ICarBrandStatisticsRepository statsRepository,
            IMemoryCache cache,
            ILogger<CarBrandQueryService> logger)
        {
            _repository = repository;
            _statsRepository = statsRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PagedResult<CarBrandResponseDto>> GetAllAsync(
            CarBrandQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var brandsQuery = _repository.GetQueryable();

            // Apply filters
            brandsQuery = ApplyFilters(brandsQuery, query);

            // Get total
            var totalCount = await brandsQuery.CountAsync(cancellationToken);

            // Apply sorting & pagination
            brandsQuery = ApplySorting(brandsQuery, query.SortBy, query.SortOrder);
            var brands = await brandsQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map with optional stats
            var dtos = await MapToDtosAsync(brands, query.IncludeStats, cancellationToken);

            return PagedResultFactory.Create(dtos, totalCount, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<CarBrandResponseDto>> GetActiveBrandsAsync(
            CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue<List<CarBrandResponseDto>>("CarBrands_Active", out var cached))
                return cached;

            var brands = await _repository.GetQueryable()
                .Where(b => b.IsActive == true)
                .OrderBy(b => b.BrandName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = brands.Select(MapToDto).ToList();
            _cache.Set("CarBrands_Active", dtos, TimeSpan.FromMinutes(5));

            return dtos;
        }

        public async Task<IEnumerable<CarBrandResponseDto>> GetBrandsByCountryAsync(
            string country,
            CancellationToken cancellationToken = default)
        {
            var brands = await _repository.GetQueryable()
                .Where(b => b.Country == country && b.IsActive == true)
                .OrderBy(b => b.BrandName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return brands.Select(MapToDto);
        }

        public async Task<IEnumerable<CarBrandResponseDto>> SearchBrandsAsync(
            string searchTerm,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveBrandsAsync(cancellationToken);

            var lower = searchTerm.ToLower();
            var brands = await _repository.GetQueryable()
                .Where(b => b.BrandName.ToLower().Contains(lower) ||
                           (b.Country != null && b.Country.ToLower().Contains(lower)))
                .OrderBy(b => b.BrandName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return brands.Select(MapToDto);
        }

        private async Task<List<CarBrandResponseDto>> MapToDtosAsync(
    List<CarBrand> brands,
    bool includeStats,
    CancellationToken cancellationToken)
        {
            var dtos = brands.Select(MapToDto).ToList();

            if (includeStats && brands.Any())
            {
                var brandIds = brands.Select(b => b.BrandId).ToList();

                var stats = await _statsRepository.GetBatchStatisticsAsync(brandIds, cancellationToken);

                foreach (var dto in dtos)
                {
                    if (stats.TryGetValue(dto.BrandId, out var brandStats))
                    {
                        dto.TotalModels = brandStats.TotalModels;
                        dto.ActiveModels = brandStats.ActiveModels;
                        dto.TotalVehicles = brandStats.TotalVehicles;
                    }
                }
            }

            return dtos;
        }
        private CarBrandResponseDto MapToDto(CarBrand brand)
        {
            return new CarBrandResponseDto
            {
                BrandId = brand.BrandId,
                BrandName = brand.BrandName,
                Country = brand.Country,
                LogoUrl = brand.LogoUrl,
                Website = brand.Website,
                Description = brand.Description,
                IsActive = brand.IsActive ?? false,
                CreatedDate = brand.CreatedDate ?? DateTime.UtcNow,
                UpdatedDate = brand.UpdatedDate
            };
        }

        private static IQueryable<CarBrand> ApplyFilters(
            IQueryable<CarBrand> query,
            CarBrandQueryDto filters)
        {
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var search = filters.SearchTerm.ToLower();
                query = query.Where(b =>
                    b.BrandName.ToLower().Contains(search) ||
                    (b.Country != null && b.Country.ToLower().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(filters.Country))
                query = query.Where(b => b.Country == filters.Country);

            if (filters.IsActive.HasValue)
                query = query.Where(b => b.IsActive == filters.IsActive.Value);

            return query;
        }

        private static IQueryable<CarBrand> ApplySorting(
            IQueryable<CarBrand> query,
            string sortBy,
            string sortOrder)
        {
            var isDesc = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "brandname" => isDesc ? query.OrderByDescending(b => b.BrandName) : query.OrderBy(b => b.BrandName),
                "country" => isDesc ? query.OrderByDescending(b => b.Country) : query.OrderBy(b => b.Country),
                "createddate" => isDesc ? query.OrderByDescending(b => b.CreatedDate) : query.OrderBy(b => b.CreatedDate),
                "isactive" => isDesc ? query.OrderByDescending(b => b.IsActive) : query.OrderBy(b => b.IsActive),
                _ => query.OrderBy(b => b.BrandName)
            };
        }

        private class BatchStats
        {
            public Dictionary<int, int> ModelCounts { get; set; } = new();
            public Dictionary<int, int> ActiveModelCounts { get; set; } = new();
            public Dictionary<int, int> VehicleCounts { get; set; } = new();
        }
    }
}