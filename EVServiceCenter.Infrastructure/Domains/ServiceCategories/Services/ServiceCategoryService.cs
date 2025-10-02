using EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Responses;
using EVServiceCenter.Core.Domains.ServiceCategories.Entities;
using EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.ServiceCategories.Services
{
    public class ServiceCategoryService : IServiceCategoryService
    {
        private readonly IServiceCategoryRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ServiceCategoryService> _logger;

        public ServiceCategoryService(
            IServiceCategoryRepository repository,
            IMemoryCache cache,
            ILogger<ServiceCategoryService> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PagedResult<ServiceCategoryResponseDto>> GetAllAsync(
            ServiceCategoryQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var categoriesQuery = _repository.GetQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.ToLower();
                categoriesQuery = categoriesQuery.Where(c =>
                    c.CategoryName.ToLower().Contains(search) ||
                    (c.Description != null && c.Description.ToLower().Contains(search)));
            }

            if (query.IsActive.HasValue)
            {
                categoriesQuery = categoriesQuery.Where(c => c.IsActive == query.IsActive.Value);
            }

            // Get total count
            var totalCount = await categoriesQuery.CountAsync(cancellationToken);

            // Apply sorting
            categoriesQuery = ApplySorting(categoriesQuery, query.SortBy, query.SortOrder);

            // Pagination
            var categories = await categoriesQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map to DTOs with statistics
            var dtos = await MapToDtosWithStatsAsync(categories, cancellationToken);

            return PagedResultFactory.Create(dtos, totalCount, query.Page, query.PageSize);
        }

        public async Task<ServiceCategoryResponseDto?> GetByIdAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"ServiceCategory_{categoryId}";
            if (_cache.TryGetValue<ServiceCategoryResponseDto>(cacheKey, out var cached))
                return cached;

            var category = await _repository.GetByIdWithDetailsAsync(categoryId, cancellationToken);
            if (category == null)
                return null;

            var dto = await MapToDtoWithStatsAsync(category, cancellationToken);
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));

            return dto;
        }

        public async Task<IEnumerable<ServiceCategoryResponseDto>> GetActiveCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            var cacheKey = "ActiveServiceCategories";
            if (_cache.TryGetValue<List<ServiceCategoryResponseDto>>(cacheKey, out var cached))
                return cached;

            var categories = await _repository.GetActiveCategoriesAsync(cancellationToken);
            var dtos = categories.Select(MapToDto).ToList();

            _cache.Set(cacheKey, dtos, TimeSpan.FromMinutes(10));

            return dtos;
        }

        public async Task<ServiceCategoryResponseDto> CreateAsync(
            CreateServiceCategoryRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Check duplicate name
            if (await _repository.IsCategoryNameExistsAsync(request.CategoryName, null, cancellationToken))
            {
                throw new InvalidOperationException($"Loại dịch vụ '{request.CategoryName}' đã tồn tại");
            }

            var category = new ServiceCategory
            {
                CategoryName = request.CategoryName.Trim(),
                Description = request.Description?.Trim(),
                IconUrl = request.IconUrl?.Trim(),
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(category);

            _logger.LogInformation("Service category created: {CategoryName} (ID: {CategoryId})",
                created.CategoryName, created.CategoryId);

            InvalidateCaches();

            return MapToDto(created);
        }

        public async Task<ServiceCategoryResponseDto> UpdateAsync(
            UpdateServiceCategoryRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(request.CategoryId);
            if (existing == null)
                throw new InvalidOperationException($"Không tìm thấy loại dịch vụ {request.CategoryId}");

            // Check duplicate name
            if (await _repository.IsCategoryNameExistsAsync(
                request.CategoryName,
                request.CategoryId,
                cancellationToken))
            {
                throw new InvalidOperationException($"Loại dịch vụ '{request.CategoryName}' đã tồn tại");
            }

            existing.CategoryName = request.CategoryName.Trim();
            existing.Description = request.Description?.Trim();
            existing.IconUrl = request.IconUrl?.Trim();
            existing.DisplayOrder = request.DisplayOrder;
            existing.IsActive = request.IsActive;
            existing.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);

            _logger.LogInformation("Service category updated: {CategoryName} (ID: {CategoryId})",
                existing.CategoryName, existing.CategoryId);

            InvalidateCaches();
            _cache.Remove($"ServiceCategory_{request.CategoryId}");

            return MapToDto(existing);
        }

        public async Task<bool> DeleteAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            if (!await _repository.CanDeleteAsync(categoryId, cancellationToken))
            {
                throw new InvalidOperationException("Không thể xóa loại dịch vụ đang có dịch vụ liên kết");
            }

            var result = await _repository.DeleteAsync(categoryId);
            if (result)
            {
                _logger.LogInformation("Service category deleted: ID {CategoryId}", categoryId);
                InvalidateCaches();
                _cache.Remove($"ServiceCategory_{categoryId}");
            }

            return result;
        }

        public Task<bool> CanDeleteAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            return _repository.CanDeleteAsync(categoryId, cancellationToken);
        }

        private async Task<List<ServiceCategoryResponseDto>> MapToDtosWithStatsAsync(
            List<ServiceCategory> categories,
            CancellationToken cancellationToken)
        {
            var dtos = new List<ServiceCategoryResponseDto>();

            // Load statistics in batch
            var categoryIds = categories.Select(c => c.CategoryId).ToList();
            var serviceStats = await _repository.GetQueryable()
                .Where(c => categoryIds.Contains(c.CategoryId))
                .Select(c => new
                {
                    c.CategoryId,
                    ServiceCount = c.MaintenanceServices.Count,
                    ActiveServiceCount = c.MaintenanceServices.Count(s => s.IsActive == true)
                })
                .ToListAsync(cancellationToken);

            foreach (var category in categories)
            {
                var stats = serviceStats.FirstOrDefault(s => s.CategoryId == category.CategoryId);
                dtos.Add(new ServiceCategoryResponseDto
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.CategoryName,
                    Description = category.Description,
                    IconUrl = category.IconUrl,
                    DisplayOrder = category.DisplayOrder ?? 0,
                    IsActive = category.IsActive ?? false,
                    CreatedDate = category.CreatedDate,
                    UpdatedDate = category.UpdatedDate,
                    ServiceCount = stats?.ServiceCount ?? 0,
                    ActiveServiceCount = stats?.ActiveServiceCount ?? 0
                });
            }

            return dtos;
        }

        private async Task<ServiceCategoryResponseDto> MapToDtoWithStatsAsync(
            ServiceCategory category,
            CancellationToken cancellationToken)
        {
            var serviceCount = category.MaintenanceServices?.Count ?? 0;
            var activeServiceCount = category.MaintenanceServices?.Count(s => s.IsActive == true) ?? 0;

            return new ServiceCategoryResponseDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                IconUrl = category.IconUrl,
                DisplayOrder = category.DisplayOrder ?? 0,
                IsActive = category.IsActive ?? false,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate,
                ServiceCount = serviceCount,
                ActiveServiceCount = activeServiceCount
            };
        }

        private ServiceCategoryResponseDto MapToDto(ServiceCategory category)
        {
            return new ServiceCategoryResponseDto
            {
                CategoryId = category.CategoryId,
                CategoryName = category.CategoryName,
                Description = category.Description,
                IconUrl = category.IconUrl,
                DisplayOrder = category.DisplayOrder ?? 0,
                IsActive = category.IsActive ?? false,
                CreatedDate = category.CreatedDate,
                UpdatedDate = category.UpdatedDate,
                ServiceCount = 0,
                ActiveServiceCount = 0
            };
        }

        private static IQueryable<ServiceCategory> ApplySorting(
            IQueryable<ServiceCategory> query,
            string sortBy,
            string sortOrder)
        {
            var isDesc = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "categoryname" => isDesc ? query.OrderByDescending(c => c.CategoryName) : query.OrderBy(c => c.CategoryName),
                "displayorder" => isDesc ? query.OrderByDescending(c => c.DisplayOrder) : query.OrderBy(c => c.DisplayOrder),
                "createddate" => isDesc ? query.OrderByDescending(c => c.CreatedDate) : query.OrderBy(c => c.CreatedDate),
                _ => query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CategoryName)
            };
        }

        private void InvalidateCaches()
        {
            _cache.Remove("ActiveServiceCategories");
        }
    }
}