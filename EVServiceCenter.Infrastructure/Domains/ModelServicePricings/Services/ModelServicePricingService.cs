using EVServiceCenter.Core.Domains.CarModels.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Requests;
using EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Responses;
using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.ModelServicePricings.Services
{
    public class ModelServicePricingService : IModelServicePricingService
    {
        private readonly IModelServicePricingRepository _repository;
        private readonly ICarModelRepository _modelRepository;
        private readonly IMaintenanceServiceRepository _serviceRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ModelServicePricingService> _logger;

        public ModelServicePricingService(
            IModelServicePricingRepository repository,
            ICarModelRepository modelRepository,
            IMaintenanceServiceRepository serviceRepository,
            IMemoryCache cache,
            ILogger<ModelServicePricingService> logger)
        {
            _repository = repository;
            _modelRepository = modelRepository;
            _serviceRepository = serviceRepository;
            _cache = cache;
            _logger = logger;
        }
        public async Task<PagedResult<ModelServicePricingResponseDto>> GetAllAsync(
    ModelServicePricingQueryDto query,
    CancellationToken cancellationToken = default)
        {
            IQueryable<ModelServicePricing> pricingQuery = _repository.GetQueryable()
                .Include(p => p.Model)
                    .ThenInclude(m => m.Brand)
                .Include(p => p.Service);

            if (query.ModelId.HasValue)
            {
                pricingQuery = pricingQuery.Where(p => p.ModelId == query.ModelId.Value);
            }

            if (query.ServiceId.HasValue)
            {
                pricingQuery = pricingQuery.Where(p => p.ServiceId == query.ServiceId.Value);
            }

            if (query.IsActive.HasValue)
            {
                pricingQuery = pricingQuery.Where(p => p.IsActive == query.IsActive.Value);
            }

            if (query.EffectiveDate.HasValue)
            {
                var checkDate = query.EffectiveDate.Value;
                pricingQuery = pricingQuery.Where(p =>
                    (!p.EffectiveDate.HasValue || p.EffectiveDate <= checkDate) &&
                    (!p.ExpiryDate.HasValue || p.ExpiryDate >= checkDate));
            }

            var totalCount = await pricingQuery.CountAsync(cancellationToken);

            pricingQuery = ApplySorting(pricingQuery, query.SortBy, query.SortOrder);

            var pricings = await pricingQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = pricings.Select(MapToDto).ToList();

            return PagedResultFactory.Create(dtos, totalCount, query.Page, query.PageSize);
        }

        public async Task<ModelServicePricingResponseDto?> GetByIdAsync(
            int pricingId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"ModelServicePricing_{pricingId}";
            if (_cache.TryGetValue<ModelServicePricingResponseDto>(cacheKey, out var cached))
                return cached;

            var pricing = await _repository.GetByIdWithDetailsAsync(pricingId, cancellationToken);
            if (pricing == null)
                return null;

            var dto = MapToDto(pricing);
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));

            return dto;
        }

        public async Task<IEnumerable<ModelServicePricingResponseDto>> GetByModelIdAsync(
            int modelId,
            CancellationToken cancellationToken = default)
        {
            var pricings = await _repository.GetByModelIdAsync(modelId, cancellationToken);
            return pricings.Select(MapToDto);
        }

        public async Task<IEnumerable<ModelServicePricingResponseDto>> GetByServiceIdAsync(
            int serviceId,
            CancellationToken cancellationToken = default)
        {
            var pricings = await _repository.GetByServiceIdAsync(serviceId, cancellationToken);
            return pricings.Select(MapToDto);
        }

        public async Task<ModelServicePricingResponseDto?> GetActivePricingAsync(
            int modelId,
            int serviceId,
            DateOnly? forDate = null,
            CancellationToken cancellationToken = default)
        {
            var pricing = await _repository.GetActivePricingAsync(modelId, serviceId, forDate, cancellationToken);
            return pricing != null ? MapToDto(pricing) : null;
        }

        public async Task<ModelServicePricingResponseDto> CreateAsync(
            CreateModelServicePricingRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Validate model exists
            var model = await _modelRepository.GetByIdAsync(request.ModelId);
            if (model == null)
                throw new InvalidOperationException($"Không tìm thấy model {request.ModelId}");

            // Validate service exists
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
            if (service == null)
                throw new InvalidOperationException($"Không tìm thấy dịch vụ {request.ServiceId}");

            // Check duplicate
            if (await _repository.IsDuplicateAsync(request.ModelId, request.ServiceId, null, cancellationToken))
            {
                throw new InvalidOperationException($"Đã có giá tùy chỉnh cho model {model.ModelName} và dịch vụ {service.ServiceName}");
            }

            var pricing = new ModelServicePricing
            {
                ModelId = request.ModelId,
                ServiceId = request.ServiceId,
                CustomPrice = request.CustomPrice,
                CustomTime = request.CustomTime,
                Notes = request.Notes?.Trim(),
                IsActive = request.IsActive,
                EffectiveDate = request.EffectiveDate,
                ExpiryDate = request.ExpiryDate,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(pricing);

            _logger.LogInformation("Model service pricing created: Model {ModelId} - Service {ServiceId}",
                request.ModelId, request.ServiceId);

            var pricingWithDetails = await _repository.GetByIdWithDetailsAsync(created.PricingId, cancellationToken);
            return MapToDto(pricingWithDetails!);
        }

        public async Task<ModelServicePricingResponseDto> UpdateAsync(
            UpdateModelServicePricingRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(request.PricingId);
            if (existing == null)
                throw new InvalidOperationException($"Không tìm thấy pricing {request.PricingId}");

            // Validate model exists
            var model = await _modelRepository.GetByIdAsync(request.ModelId);
            if (model == null)
                throw new InvalidOperationException($"Không tìm thấy model {request.ModelId}");

            // Validate service exists
            var service = await _serviceRepository.GetByIdAsync(request.ServiceId);
            if (service == null)
                throw new InvalidOperationException($"Không tìm thấy dịch vụ {request.ServiceId}");

            // Check duplicate
            if (await _repository.IsDuplicateAsync(request.ModelId, request.ServiceId, request.PricingId, cancellationToken))
            {
                throw new InvalidOperationException($"Đã có giá tùy chỉnh cho model {model.ModelName} và dịch vụ {service.ServiceName}");
            }

            existing.ModelId = request.ModelId;
            existing.ServiceId = request.ServiceId;
            existing.CustomPrice = request.CustomPrice;
            existing.CustomTime = request.CustomTime;
            existing.Notes = request.Notes?.Trim();
            existing.IsActive = request.IsActive;
            existing.EffectiveDate = request.EffectiveDate;
            existing.ExpiryDate = request.ExpiryDate;
            existing.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);

            _logger.LogInformation("Model service pricing updated: {PricingId}", request.PricingId);

            _cache.Remove($"ModelServicePricing_{request.PricingId}");

            var updated = await _repository.GetByIdWithDetailsAsync(request.PricingId, cancellationToken);
            return MapToDto(updated!);
        }

        public async Task<bool> DeleteAsync(
            int pricingId,
            CancellationToken cancellationToken = default)
        {
            var result = await _repository.DeleteAsync(pricingId);
            if (result)
            {
                _logger.LogInformation("Model service pricing deleted: {PricingId}", pricingId);
                _cache.Remove($"ModelServicePricing_{pricingId}");
            }

            return result;
        }

        private ModelServicePricingResponseDto MapToDto(ModelServicePricing pricing)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var isCurrentlyActive = pricing.IsActive == true &&
                (!pricing.EffectiveDate.HasValue || pricing.EffectiveDate <= today) &&
                (!pricing.ExpiryDate.HasValue || pricing.ExpiryDate >= today);

            return new ModelServicePricingResponseDto
            {
                PricingId = pricing.PricingId,
                ModelId = pricing.ModelId,
                ModelName = pricing.Model?.ModelName ?? string.Empty,
                BrandName = pricing.Model?.Brand?.BrandName ?? string.Empty,
                ServiceId = pricing.ServiceId,
                ServiceCode = pricing.Service?.ServiceCode ?? string.Empty,
                ServiceName = pricing.Service?.ServiceName ?? string.Empty,
                CustomPrice = pricing.CustomPrice,
                BasePrice = pricing.Service?.BasePrice ?? 0,
                FinalPrice = pricing.CustomPrice ?? pricing.Service?.BasePrice ?? 0,
                CustomTime = pricing.CustomTime,
                StandardTime = pricing.Service?.StandardTime ?? 0,
                FinalTime = pricing.CustomTime ?? pricing.Service?.StandardTime ?? 0,
                Notes = pricing.Notes,
                IsActive = pricing.IsActive ?? false,
                EffectiveDate = pricing.EffectiveDate,
                ExpiryDate = pricing.ExpiryDate,
                IsCurrentlyActive = isCurrentlyActive,
                CreatedDate = pricing.CreatedDate,
                UpdatedDate = pricing.UpdatedDate
            };
        }

        private static IQueryable<ModelServicePricing> ApplySorting(
            IQueryable<ModelServicePricing> query,
            string sortBy,
            string sortOrder)
        {
            var isDesc = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "pricingid" => isDesc ? query.OrderByDescending(p => p.PricingId) : query.OrderBy(p => p.PricingId),
                "modelname" => isDesc ? query.OrderByDescending(p => p.Model.ModelName) : query.OrderBy(p => p.Model.ModelName),
                "servicename" => isDesc ? query.OrderByDescending(p => p.Service.ServiceName) : query.OrderBy(p => p.Service.ServiceName),
                "customprice" => isDesc ? query.OrderByDescending(p => p.CustomPrice) : query.OrderBy(p => p.CustomPrice),
                "effectivedate" => isDesc ? query.OrderByDescending(p => p.EffectiveDate) : query.OrderBy(p => p.EffectiveDate),
                _ => query.OrderBy(p => p.PricingId)
            };
        }
    }
}