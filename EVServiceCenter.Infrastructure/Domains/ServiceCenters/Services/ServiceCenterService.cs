using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Responses;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services;
using EVServiceCenter.Core.Exceptions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.ServiceCenters.Services
{
    public class ServiceCenterService : IServiceCenterService
    {
        private readonly IServiceCenterRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ServiceCenterService> _logger;

        public ServiceCenterService(
            IServiceCenterRepository repository,
            IMemoryCache cache,
            ILogger<ServiceCenterService> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<ServiceCenterResponseDto?> GetByIdAsync(
            int centerId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.GetByIdKey(centerId, false);
            if (_cache.TryGetValue<ServiceCenterResponseDto>(cacheKey, out var cachedDto))
                return cachedDto;

            var center = await _repository.GetByIdWithDetailsAsync(centerId, cancellationToken);
            if (center == null)
                return null;

            var dto = MapToDto(center);

            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));

            return dto;
        }

        public async Task<ServiceCenterResponseDto?> GetByCenterCodeAsync(
            string centerCode,
            CancellationToken cancellationToken = default)
        {
            var center = await _repository.GetByCenterCodeAsync(centerCode, cancellationToken);
            return center != null ? MapToDto(center) : null;
        }

        public async Task<ServiceCenterResponseDto> CreateAsync(
     CreateServiceCenterRequestDto request,
     CancellationToken cancellationToken = default)
        {
            if (await _repository.IsCenterCodeExistsAsync(request.CenterCode, null, cancellationToken))
            {
                throw new BusinessRuleException(
                    $"Mã trung tâm '{request.CenterCode}' đã tồn tại trong hệ thống",
                    "DUPLICATE_CENTER_CODE"
                );
            }

            var center = MapToEntity(request);
            var created = await _repository.CreateAsync(center);

            _logger.LogInformation("Service center created: {Code}", created.CenterCode);
            InvalidateListCaches();

            return MapToDto(created);
        }

        public async Task<ServiceCenterResponseDto> UpdateAsync(
            UpdateServiceCenterRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(request.CenterId);
            if (existing == null)
            {
                throw new BusinessRuleException(
                    $"Không tìm thấy trung tâm có ID = {request.CenterId}",
                    "CENTER_NOT_FOUND"
                );
            }

            if (await _repository.IsCenterCodeExistsAsync(request.CenterCode, request.CenterId, cancellationToken))
            {
                throw new BusinessRuleException(
                    $"Mã trung tâm '{request.CenterCode}' đã được sử dụng bởi trung tâm khác",
                    "DUPLICATE_CENTER_CODE"
                );
            }

            UpdateEntity(existing, request);
            var updated = await _repository.UpdateAsync(existing);

            InvalidateCenterCaches(request.CenterId);

            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(
            int centerId,
            CancellationToken cancellationToken = default)
        {
            if (!await _repository.CanDeleteAsync(centerId, cancellationToken))
            {
                throw new BusinessRuleException(
                    "Không thể xóa trung tâm vì có dữ liệu liên quan (appointments, work orders, hoặc departments)",
                    "CANNOT_DELETE_HAS_REFERENCES"
                );
            }

            var result = await _repository.DeleteAsync(centerId);
            if (result)
            {
                InvalidateCenterCaches(centerId);
            }

            return result;
        }

        public Task<bool> CanDeleteAsync(int centerId, CancellationToken cancellationToken = default)
        {
            return _repository.CanDeleteAsync(centerId, cancellationToken);
        }

        public Task<bool> IsCenterCodeExistsAsync(
            string centerCode,
            int? excludeCenterId = null,
            CancellationToken cancellationToken = default)
        {
            return _repository.IsCenterCodeExistsAsync(centerCode, excludeCenterId, cancellationToken);
        }
        private ServiceCenterResponseDto MapToDto(ServiceCenter center)
        {
            return new ServiceCenterResponseDto
            {
                CenterId = center.CenterId,
                CenterName = center.CenterName,
                CenterCode = center.CenterCode,

                // Address
                Address = center.Address ?? string.Empty,
                Ward = center.Ward,
                District = center.District,
                Province = center.Province,
                PostalCode = center.PostalCode,
                FullAddress = BuildFullAddress(center),

                // Contact
                PhoneNumber = center.PhoneNumber ?? string.Empty,
                Email = center.Email,
                Website = center.Website,

                // GPS
                Latitude = center.Latitude,
                Longitude = center.Longitude,

                // Working hours
                OpenTime = center.OpenTime,
                CloseTime = center.CloseTime,
                WorkingHours = $"{center.OpenTime:HH:mm} - {center.CloseTime:HH:mm}",

                // Operational
                Capacity = center.Capacity ?? 0,
                Description = center.Description,
                Facilities = center.Facilities,

                // Management
                ManagerId = center.ManagerId,
                ManagerName = center.Manager?.FullName,

                // Status
                IsActive = center.IsActive ?? false,
                CreatedDate = center.CreatedDate ?? DateTime.UtcNow
            };
        }

        private ServiceCenter MapToEntity(CreateServiceCenterRequestDto request)
        {
            return new ServiceCenter
            {
                CenterName = request.CenterName,
                CenterCode = request.CenterCode,

                // Address
                Address = request.Address,
                Ward = request.Ward,
                District = request.District,
                Province = request.Province,
                PostalCode = request.PostalCode,

                // Contact
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Website = request.Website,

                // GPS
                Latitude = request.Latitude,
                Longitude = request.Longitude,

                // Working hours
                OpenTime = request.OpenTime,
                CloseTime = request.CloseTime,

                // Operational
                Capacity = request.Capacity,
                Description = request.Description,
                Facilities = request.Facilities,

                // Management
                ManagerId = request.ManagerId,

                // Status
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow
            };
        }

        private void UpdateEntity(ServiceCenter entity, UpdateServiceCenterRequestDto request)
        {
            entity.CenterName = request.CenterName;
            entity.CenterCode = request.CenterCode;

            // Address
            entity.Address = request.Address;
            entity.Ward = request.Ward;
            entity.District = request.District;
            entity.Province = request.Province;
            entity.PostalCode = request.PostalCode;

            // Contact
            entity.PhoneNumber = request.PhoneNumber;
            entity.Email = request.Email;
            entity.Website = request.Website;

            // GPS
            entity.Latitude = request.Latitude;
            entity.Longitude = request.Longitude;

            // Working hours
            entity.OpenTime = request.OpenTime;
            entity.CloseTime = request.CloseTime;

            // Operational
            entity.Capacity = request.Capacity;
            entity.Description = request.Description;
            entity.Facilities = request.Facilities;

            // Management
            entity.ManagerId = request.ManagerId;

            // Status
            entity.IsActive = request.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;
        }

        private static string BuildFullAddress(ServiceCenter center)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(center.Address)) parts.Add(center.Address);
            if (!string.IsNullOrWhiteSpace(center.Ward)) parts.Add(center.Ward);
            if (!string.IsNullOrWhiteSpace(center.District)) parts.Add(center.District);
            if (!string.IsNullOrWhiteSpace(center.Province)) parts.Add(center.Province);
            return string.Join(", ", parts);
        }

        private void InvalidateCenterCaches(int centerId)
        {
            _cache.Remove(CacheKeys.GetByIdKey(centerId, false));
            _cache.Remove(CacheKeys.GetByIdKey(centerId, true));
            InvalidateListCaches();
        }

        private void InvalidateListCaches()
        {
            _cache.Remove(CacheKeys.SERVICE_CENTER_ACTIVE);
            _cache.Remove(CacheKeys.SERVICE_CENTER_ALL_STATS);
        }
    }
}