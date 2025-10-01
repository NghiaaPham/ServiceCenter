using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarModels.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarModels.DTOs.Responses;
using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.CarModels.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarModels.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Application.Domains.CarModels.Services
{
    public class CarModelService : ICarModelService
    {
        private readonly ICarModelRepository _repository;
        private readonly ICarBrandRepository _brandRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CarModelService> _logger;

        public CarModelService(
            ICarModelRepository repository,
            ICarBrandRepository brandRepository,
            IMemoryCache cache,
            ILogger<CarModelService> logger)
        {
            _repository = repository;
            _brandRepository = brandRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CarModelResponseDto?> GetByIdAsync(
            int modelId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"CarModel_{modelId}";
            if (_cache.TryGetValue<CarModelResponseDto>(cacheKey, out var cached))
                return cached;

            var model = await _repository.GetByIdWithBrandAsync(modelId, cancellationToken);
            if (model == null)
                return null;

            var dto = MapToDto(model);
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));

            return dto;
        }

        public async Task<CarModelResponseDto> CreateAsync(
            CreateCarModelRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // Validate brand exists
            var brand = await _brandRepository.GetByIdAsync(request.BrandId);
            if (brand == null)
                throw new InvalidOperationException($"Không tìm thấy thương hiệu {request.BrandId}");

            // Check duplicate
            if (await _repository.IsModelNameExistsAsync(request.BrandId, request.ModelName, null, cancellationToken))
            {
                throw new InvalidOperationException($"Dòng xe '{request.ModelName}' đã tồn tại cho thương hiệu này");
            }

            var model = new CarModel
            {
                BrandId = request.BrandId,
                ModelName = request.ModelName,
                Year = request.Year,
                BatteryCapacity = request.BatteryCapacity,
                MaxRange = request.MaxRange,
                ChargingType = request.ChargingType,
                MotorPower = request.MotorPower,
                AccelerationTime = request.AccelerationTime,
                TopSpeed = request.TopSpeed,
                ServiceInterval = request.ServiceInterval,
                ServiceIntervalMonths = request.ServiceIntervalMonths,
                WarrantyPeriod = request.WarrantyPeriod,
                ImageUrl = request.ImageUrl,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(model);

            // Load brand for response
            var modelWithBrand = await _repository.GetByIdWithBrandAsync(created.ModelId, cancellationToken);

            _logger.LogInformation("Car model created: {BrandName} {ModelName}",
                brand.BrandName, created.ModelName);
            InvalidateListCaches();

            return MapToDto(modelWithBrand!);
        }

        public async Task<CarModelResponseDto> UpdateAsync(
            UpdateCarModelRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(request.ModelId);
            if (existing == null)
                throw new InvalidOperationException($"Không tìm thấy dòng xe {request.ModelId}");

            // Validate brand exists
            var brand = await _brandRepository.GetByIdAsync(request.BrandId);
            if (brand == null)
                throw new InvalidOperationException($"Không tìm thấy thương hiệu {request.BrandId}");

            // Check duplicate
            if (await _repository.IsModelNameExistsAsync(
                request.BrandId,
                request.ModelName,
                request.ModelId,
                cancellationToken))
            {
                throw new InvalidOperationException($"Tên dòng xe '{request.ModelName}' đã được sử dụng");
            }

            existing.BrandId = request.BrandId;
            existing.ModelName = request.ModelName;
            existing.Year = request.Year;
            existing.BatteryCapacity = request.BatteryCapacity;
            existing.MaxRange = request.MaxRange;
            existing.ChargingType = request.ChargingType;
            existing.MotorPower = request.MotorPower;
            existing.AccelerationTime = request.AccelerationTime;
            existing.TopSpeed = request.TopSpeed;
            existing.ServiceInterval = request.ServiceInterval;
            existing.ServiceIntervalMonths = request.ServiceIntervalMonths;
            existing.WarrantyPeriod = request.WarrantyPeriod;
            existing.ImageUrl = request.ImageUrl;
            existing.Description = request.Description;
            existing.IsActive = request.IsActive;
            existing.UpdatedDate = DateTime.UtcNow;

            await _repository.UpdateAsync(existing);

            var updated = await _repository.GetByIdWithBrandAsync(request.ModelId, cancellationToken);

            _logger.LogInformation("Car model updated: {ModelId}", request.ModelId);
            InvalidateModelCache(request.ModelId);

            return MapToDto(updated!);
        }

        public async Task<bool> DeleteAsync(
            int modelId,
            CancellationToken cancellationToken = default)
        {
            if (!await _repository.CanDeleteAsync(modelId, cancellationToken))
            {
                throw new InvalidOperationException("Không thể xóa vì có xe hoặc bảng giá liên quan");
            }

            var result = await _repository.DeleteAsync(modelId);
            if (result)
            {
                InvalidateModelCache(modelId);
            }

            return result;
        }

        public Task<bool> CanDeleteAsync(int modelId, CancellationToken cancellationToken = default)
        {
            return _repository.CanDeleteAsync(modelId, cancellationToken);
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

        private void InvalidateModelCache(int modelId)
        {
            _cache.Remove($"CarModel_{modelId}");
            InvalidateListCaches();
        }

        private void InvalidateListCaches()
        {
            _cache.Remove("CarModels_Active");
            _cache.Remove("CarModels_AllStats");
        }
    }
}