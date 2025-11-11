using EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Responses;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Services;
using EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;


namespace EVServiceCenter.Infrastructure.Domains.MaintenanceServices.Services
{
    public class MaintenanceServiceService : IMaintenanceServiceService
    {
        private readonly IMaintenanceServiceRepository _repository;
        private readonly IServiceCategoryRepository _categoryRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<MaintenanceServiceService> _logger;

        public MaintenanceServiceService(
            IMaintenanceServiceRepository repository,
            IServiceCategoryRepository categoryRepository,
            IMemoryCache cache,
            ILogger<MaintenanceServiceService> logger)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PagedResult<MaintenanceServiceResponseDto>> GetAllAsync(
           MaintenanceServiceQueryDto query,
           CancellationToken cancellationToken = default)
        {
            IQueryable<MaintenanceService> servicesQuery = _repository.GetQueryable()
                .Include(s => s.Category)
                .Include(s => s.ModelServicePricings); // include pricing info for model filtering

            // Apply filters
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.ToLower();
                servicesQuery = servicesQuery.Where(s =>
                    s.ServiceName.ToLower().Contains(search) ||
                    s.ServiceCode.ToLower().Contains(search) ||
                    (s.Description != null && s.Description.ToLower().Contains(search)));
            }

            if (query.CategoryId.HasValue)
            {
                servicesQuery = servicesQuery.Where(s => s.CategoryId == query.CategoryId.Value);
            }

            if (query.ModelId.HasValue)
            {
                // Filter services that have a model-specific pricing or are applicable to the model
                var modelId = query.ModelId.Value;
                servicesQuery = servicesQuery.Where(s => s.ModelServicePricings.Any(mp => mp.ModelId == modelId && (mp.IsActive == true || mp.IsActive == null)));
            }

            if (query.IsActive.HasValue)
            {
                servicesQuery = servicesQuery.Where(s => s.IsActive == query.IsActive.Value);
            }

            if (query.IsWarrantyService.HasValue)
            {
                servicesQuery = servicesQuery.Where(s => s.IsWarrantyService == query.IsWarrantyService.Value);
            }

            if (query.MinPrice.HasValue)
            {
                servicesQuery = servicesQuery.Where(s => s.BasePrice >= query.MinPrice.Value);
            }

            if (query.MaxPrice.HasValue)
            {
                servicesQuery = servicesQuery.Where(s => s.BasePrice <= query.MaxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SkillLevel))
            {
                servicesQuery = servicesQuery.Where(s => s.SkillLevel == query.SkillLevel);
            }

            // Get total count
            var totalCount = await servicesQuery.CountAsync(cancellationToken);

            // Apply sorting
            servicesQuery = ApplySorting(servicesQuery, query.SortBy, query.SortOrder);

            // Pagination
            var services = await servicesQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map to DTOs
            var dtos = await MapToDtosWithStatsAsync(services, query.ModelId, cancellationToken);

            return PagedResultFactory.Create(dtos, totalCount, query.Page, query.PageSize);
        }


        public async Task<MaintenanceServiceResponseDto?> GetByIdAsync(
            int serviceId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"MaintenanceService_{serviceId}";
            if (_cache.TryGetValue<MaintenanceServiceResponseDto>(cacheKey, out var cached))
                return cached;

            var service = await _repository.GetByIdWithDetailsAsync(serviceId, cancellationToken);
            if (service == null)
                return null;

            var dto = await MapToDtoWithStatsAsync(service, cancellationToken);
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));

            return dto;
        }

        public async Task<IEnumerable<MaintenanceServiceResponseDto>> GetActiveServicesAsync(
            CancellationToken cancellationToken = default)
        {
            var cacheKey = "ActiveMaintenanceServices";
            if (_cache.TryGetValue<List<MaintenanceServiceResponseDto>>(cacheKey, out var cached))
                return cached;

            var services = await _repository.GetActiveServicesAsync(cancellationToken);
            var dtos = services.Select(MapToDto).ToList();

            _cache.Set(cacheKey, dtos, TimeSpan.FromMinutes(10));

            return dtos;
        }

        public async Task<IEnumerable<MaintenanceServiceResponseDto>> GetServicesByCategoryAsync(
            int categoryId,
            CancellationToken cancellationToken = default)
        {
            var services = await _repository.GetServicesByCategoryAsync(categoryId, cancellationToken);
            return services.Select(MapToDto);
        }

        public async Task<MaintenanceServiceResponseDto> CreateAsync(
            CreateMaintenanceServiceRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Validate category exists
            var categoryExists = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (categoryExists == null)
                throw new InvalidOperationException($"Không tìm thấy loại dịch vụ {request.CategoryId}");

            // Check duplicate service code
            if (await _repository.IsServiceCodeExistsAsync(request.ServiceCode, null, cancellationToken))
            {
                throw new InvalidOperationException($"Mã dịch vụ '{request.ServiceCode}' đã tồn tại");
            }

            var service = new MaintenanceService
            {
                CategoryId = request.CategoryId,
                ServiceCode = request.ServiceCode.ToUpper(),
                ServiceName = request.ServiceName.Trim(),
                Description = request.Description?.Trim(),
                StandardTime = request.StandardTime,
                BasePrice = request.BasePrice,
                LaborCost = request.LaborCost,
                SkillLevel = request.SkillLevel?.Trim(),
                RequiredCertification = request.RequiredCertification?.Trim(),
                IsWarrantyService = request.IsWarrantyService,
                WarrantyPeriod = request.WarrantyPeriod,
                ImageUrl = request.ImageUrl?.Trim(),
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(service);

            _logger.LogInformation("Maintenance service created: {ServiceCode} - {ServiceName}",
                created.ServiceCode, created.ServiceName);

            InvalidateCaches();

            var serviceWithDetails = await _repository.GetByIdWithDetailsAsync(created.ServiceId, cancellationToken);
            return MapToDto(serviceWithDetails!);
        }

        public async Task<MaintenanceServiceResponseDto> UpdateAsync(
            UpdateMaintenanceServiceRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(request.ServiceId);
            if (existing == null)
                throw new InvalidOperationException($"Không tìm thấy dịch vụ {request.ServiceId}");

            // Validate category exists
            var categoryExists = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (categoryExists == null)
                throw new InvalidOperationException($"Không tìm thấy loại dịch vụ {request.CategoryId}");

            // Check duplicate service code
            if (await _repository.IsServiceCodeExistsAsync(
                request.ServiceCode,
                request.ServiceId,
                cancellationToken))
            {
                throw new InvalidOperationException($"Mã dịch vụ '{request.ServiceCode}' đã được sử dụng");
            }

            existing.CategoryId = request.CategoryId;
            existing.ServiceCode = request.ServiceCode.ToUpper();
            existing.ServiceName = request.ServiceName.Trim();
            existing.Description = request.Description?.Trim();
            existing.StandardTime = request.StandardTime;
            existing.BasePrice = request.BasePrice;
            existing.LaborCost = request.LaborCost;
            existing.SkillLevel = request.SkillLevel?.Trim();
            existing.RequiredCertification = request.RequiredCertification?.Trim();
            existing.IsWarrantyService = request.IsWarrantyService;
            existing.WarrantyPeriod = request.WarrantyPeriod;
            existing.ImageUrl = request.ImageUrl?.Trim();
            existing.IsActive = request.IsActive;
            existing.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);

            _logger.LogInformation("Maintenance service updated: {ServiceId}", request.ServiceId);

            InvalidateCaches();
            _cache.Remove($"MaintenanceService_{request.ServiceId}");

            var updated = await _repository.GetByIdWithDetailsAsync(request.ServiceId, cancellationToken);
            return MapToDto(updated!);
        }

        public async Task<bool> DeleteAsync(
            int serviceId,
            CancellationToken cancellationToken = default)
        {
            if (!await _repository.CanDeleteAsync(serviceId, cancellationToken))
            {
                throw new InvalidOperationException("Không thể xóa dịch vụ đang có lịch hẹn, phiếu công việc hoặc gói dịch vụ liên kết");
            }

            var result = await _repository.DeleteAsync(serviceId);
            if (result)
            {
                _logger.LogInformation("Maintenance service deleted: ID {ServiceId}", serviceId);
                InvalidateCaches();
                _cache.Remove($"MaintenanceService_{serviceId}");
            }

            return result;
        }

        public Task<bool> CanDeleteAsync(
            int serviceId,
            CancellationToken cancellationToken = default)
        {
            return _repository.CanDeleteAsync(serviceId, cancellationToken);
        }

        private async Task<List<MaintenanceServiceResponseDto>> MapToDtosWithStatsAsync(
            List<MaintenanceService> services,
            int? modelId,
            CancellationToken cancellationToken)
        {
            var dtos = new List<MaintenanceServiceResponseDto>();
            var serviceIds = services.Select(s => s.ServiceId).ToList();

            // Load statistics in batch
            var appointmentStats = await _repository.GetQueryable()
                .Where(s => serviceIds.Contains(s.ServiceId))
                .Select(s => new
                {
                    s.ServiceId,
                    AppointmentCount = s.Appointments.Count,
                    WorkOrderCount = s.WorkOrderServices.Count
                })
                .ToListAsync(cancellationToken);

            // If modelId provided, get set of serviceIds that have model-specific pricing
            HashSet<int> modelPricingServiceIds = new HashSet<int>();
            if (modelId.HasValue)
            {
                var modelServices = await _repository.GetQueryable()
                    .Where(s => serviceIds.Contains(s.ServiceId) && s.ModelServicePricings.Any(mp => mp.ModelId == modelId.Value && (mp.IsActive == true || mp.IsActive == null)))
                    .Select(s => s.ServiceId)
                    .ToListAsync(cancellationToken);

                modelPricingServiceIds = new HashSet<int>(modelServices);
            }

            foreach (var service in services)
            {
                var stats = appointmentStats.FirstOrDefault(a => a.ServiceId == service.ServiceId);
                var dto = MapToDto(service);
                dto.AppointmentCount = stats?.AppointmentCount ?? 0;
                dto.WorkOrderCount = stats?.WorkOrderCount ?? 0;

                // set HasModelPricing flag for frontend convenience
                dto.HasModelPricing = modelId.HasValue && modelPricingServiceIds.Contains(service.ServiceId);

                dtos.Add(dto);
            }

            return dtos;
        }

        private async Task<MaintenanceServiceResponseDto> MapToDtoWithStatsAsync(
            MaintenanceService service,
            CancellationToken cancellationToken)
        {
            var dto = MapToDto(service);
            dto.AppointmentCount = service.Appointments?.Count ?? 0;
            dto.WorkOrderCount = service.WorkOrderServices?.Count ?? 0;
            // HasModelPricing unknown for single-get (requires model context)
            dto.HasModelPricing = false;
            return dto;
        }

        private MaintenanceServiceResponseDto MapToDto(MaintenanceService service)
        {
            return new MaintenanceServiceResponseDto
            {
                ServiceId = service.ServiceId,
                CategoryId = service.CategoryId,
                CategoryName = service.Category?.CategoryName ?? string.Empty,
                ServiceCode = service.ServiceCode,
                ServiceName = service.ServiceName,
                Description = service.Description,
                StandardTime = service.StandardTime,
                BasePrice = service.BasePrice,
                LaborCost = service.LaborCost,
                TotalCost = service.BasePrice + (service.LaborCost ?? 0),
                SkillLevel = service.SkillLevel,
                RequiredCertification = service.RequiredCertification,
                IsWarrantyService = service.IsWarrantyService ?? false,
                WarrantyPeriod = service.WarrantyPeriod,
                ImageUrl = service.ImageUrl,
                IsActive = service.IsActive ?? false,
                CreatedDate = service.CreatedDate,
                UpdatedDate = service.UpdatedDate,
                AppointmentCount = 0,
                WorkOrderCount = 0,
                HasModelPricing = false
            };
        }

        private static IQueryable<MaintenanceService> ApplySorting(
            IQueryable<MaintenanceService> query,
            string sortBy,
            string sortOrder)
        {
            var isDesc = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "servicename" => isDesc ? query.OrderByDescending(s => s.ServiceName) : query.OrderBy(s => s.ServiceName),
                "servicecode" => isDesc ? query.OrderByDescending(s => s.ServiceCode) : query.OrderBy(s => s.ServiceCode),
                "baseprice" => isDesc ? query.OrderByDescending(s => s.BasePrice) : query.OrderBy(s => s.BasePrice),
                "standardtime" => isDesc ? query.OrderByDescending(s => s.StandardTime) : query.OrderBy(s => s.StandardTime),
                "categoryname" => isDesc ? query.OrderByDescending(s => s.Category.CategoryName) : query.OrderBy(s => s.Category.CategoryName),
                "createddate" => isDesc ? query.OrderByDescending(s => s.CreatedDate) : query.OrderBy(s => s.CreatedDate),
                _ => query.OrderBy(s => s.ServiceName)
            };
        }

        private void InvalidateCaches()
        {
            _cache.Remove("ActiveMaintenanceServices");
        }
    }
}