using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.CarBrands.DTOs.Requests;
using EVServiceCenter.Core.Domains.CarBrands.DTOs.Responses;
using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarBrands.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Application.Domains.CarBrands.Services
{
    public class CarBrandService : ICarBrandService
    {
        private readonly ICarBrandRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CarBrandService> _logger;

        public CarBrandService(
            ICarBrandRepository repository,
            IMemoryCache cache,
            ILogger<CarBrandService> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CarBrandResponseDto?> GetByIdAsync(
            int brandId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"CarBrand_{brandId}";
            if (_cache.TryGetValue<CarBrandResponseDto>(cacheKey, out var cached))
                return cached;

            var brand = await _repository.GetByIdAsync(brandId);
            if (brand == null)
                return null;

            var dto = MapToDto(brand);
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));

            return dto;
        }

        public async Task<CarBrandResponseDto?> GetByNameAsync(
            string brandName,
            CancellationToken cancellationToken = default)
        {
            var brand = await _repository.GetByNameAsync(brandName, cancellationToken);
            return brand != null ? MapToDto(brand) : null;
        }

        public async Task<CarBrandResponseDto> CreateAsync(
            CreateCarBrandRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (await _repository.IsBrandNameExistsAsync(request.BrandName, null, cancellationToken))
            {
                throw new InvalidOperationException($"Thương hiệu '{request.BrandName}' đã tồn tại");
            }

            var brand = new CarBrand
            {
                BrandName = request.BrandName,
                Country = request.Country,
                LogoUrl = request.LogoUrl,
                Website = request.Website,
                Description = request.Description,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(brand);

            _logger.LogInformation("Car brand created: {BrandName}", created.BrandName);
            InvalidateListCaches();

            return MapToDto(created);
        }

        public async Task<CarBrandResponseDto> UpdateAsync(
            UpdateCarBrandRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(request.BrandId);
            if (existing == null)
                throw new InvalidOperationException($"Không tìm thấy thương hiệu {request.BrandId}");

            if (await _repository.IsBrandNameExistsAsync(request.BrandName, request.BrandId, cancellationToken))
            {
                throw new InvalidOperationException($"Tên thương hiệu '{request.BrandName}' đã được sử dụng");
            }

            existing.BrandName = request.BrandName;
            existing.Country = request.Country;
            existing.LogoUrl = request.LogoUrl;
            existing.Website = request.Website;
            existing.Description = request.Description;
            existing.IsActive = request.IsActive;
            existing.UpdatedDate = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(existing);

            _logger.LogInformation("Car brand updated: {BrandId}", request.BrandId);
            InvalidateBrandCache(request.BrandId);

            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(
            int brandId,
            CancellationToken cancellationToken = default)
        {
            if (!await _repository.CanDeleteAsync(brandId, cancellationToken))
            {
                throw new InvalidOperationException("Không thể xóa vì có dòng xe liên quan");
            }

            var result = await _repository.DeleteAsync(brandId);
            if (result)
            {
                InvalidateBrandCache(brandId);
            }

            return result;
        }

        public Task<bool> CanDeleteAsync(int brandId, CancellationToken cancellationToken = default)
        {
            return _repository.CanDeleteAsync(brandId, cancellationToken);
        }

        public Task<bool> IsBrandNameExistsAsync(
            string brandName,
            int? excludeBrandId = null,
            CancellationToken cancellationToken = default)
        {
            return _repository.IsBrandNameExistsAsync(brandName, excludeBrandId, cancellationToken);
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

        private void InvalidateBrandCache(int brandId)
        {
            _cache.Remove($"CarBrand_{brandId}");
            InvalidateListCaches();
        }

        private void InvalidateListCaches()
        {
            _cache.Remove("CarBrands_Active");
            _cache.Remove("CarBrands_AllStats");
        }
    }
}