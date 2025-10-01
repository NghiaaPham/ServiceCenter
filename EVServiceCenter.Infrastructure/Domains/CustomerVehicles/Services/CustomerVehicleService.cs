using EVServiceCenter.Core.Domains.CarModels.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Responses;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.CustomerVehicles.Services
{
    public class CustomerVehicleService : ICustomerVehicleService
    {
        private readonly ICustomerVehicleRepository _repository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICarModelRepository _modelRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CustomerVehicleService> _logger;

        public CustomerVehicleService(
            ICustomerVehicleRepository repository,
            ICustomerRepository customerRepository,
            ICarModelRepository modelRepository,
            IMemoryCache cache,
            ILogger<CustomerVehicleService> logger)
        {
            _repository = repository;
            _customerRepository = customerRepository;
            _modelRepository = modelRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<CustomerVehicleResponseDto?> GetByIdAsync(
            int vehicleId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"Vehicle_{vehicleId}";
            if (_cache.TryGetValue<CustomerVehicleResponseDto>(cacheKey, out var cached))
                return cached;

            var vehicle = await _repository.GetByIdWithDetailsAsync(vehicleId, cancellationToken);
            if (vehicle == null)
                return null;

            var dto = MapToDto(vehicle);
            _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));

            return dto;
        }

        public async Task<CustomerVehicleResponseDto> CreateAsync(
            CreateCustomerVehicleRequestDto request,
            int createdByUserId,
            CancellationToken cancellationToken = default)
        {
            // Validate customer exists - single query
            var customerExists = await _customerRepository.ExistsAsync(
                phoneNumber: string.Empty,
                excludeCustomerId: request.CustomerId,
                cancellationToken);

            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, false, cancellationToken);
            if (customer == null)
                throw new InvalidOperationException($"Không tìm thấy khách hàng {request.CustomerId}");

            // Validate model exists - single query
            var model = await _modelRepository.GetByIdAsync(request.ModelId);
            if (model == null)
                throw new InvalidOperationException($"Không tìm thấy dòng xe {request.ModelId}");

            // Check duplicate license plate - single query
            if (await _repository.IsLicensePlateExistsAsync(request.LicensePlate, null, cancellationToken))
            {
                throw new InvalidOperationException($"Biển số xe '{request.LicensePlate}' đã tồn tại");
            }

            // Check duplicate VIN if provided - single query
            if (!string.IsNullOrEmpty(request.Vin) &&
                await _repository.IsVinExistsAsync(request.Vin, null, cancellationToken))
            {
                throw new InvalidOperationException($"VIN '{request.Vin}' đã tồn tại");
            }

            var vehicle = new CustomerVehicle
            {
                CustomerId = request.CustomerId,
                ModelId = request.ModelId,
                LicensePlate = request.LicensePlate.ToUpper(),
                Vin = request.Vin?.ToUpper(),
                Color = request.Color,
                PurchaseDate = request.PurchaseDate,
                Mileage = request.Mileage,
                LastMaintenanceDate = request.LastMaintenanceDate,
                NextMaintenanceDate = request.NextMaintenanceDate,
                LastMaintenanceMileage = request.LastMaintenanceMileage,
                NextMaintenanceMileage = request.NextMaintenanceMileage,
                BatteryHealthPercent = request.BatteryHealthPercent,
                VehicleCondition = request.VehicleCondition,
                InsuranceNumber = request.InsuranceNumber,
                InsuranceExpiry = request.InsuranceExpiry,
                RegistrationExpiry = request.RegistrationExpiry,
                Notes = request.Notes,
                IsActive = request.IsActive,
                CreatedDate = DateTime.UtcNow,
                UpdatedBy = createdByUserId
            };

            var created = await _repository.CreateAsync(vehicle);

            // Load with details for response - single query with includes
            var vehicleWithDetails = await _repository.GetByIdWithDetailsAsync(created.VehicleId, cancellationToken);

            _logger.LogInformation("Vehicle created: {LicensePlate} for customer {CustomerId}",
                created.LicensePlate, created.CustomerId);

            InvalidateListCaches();

            return MapToDto(vehicleWithDetails!);
        }

        public async Task<CustomerVehicleResponseDto> UpdateAsync(
            UpdateCustomerVehicleRequestDto request,
            int updatedByUserId,
            CancellationToken cancellationToken = default)
        {
            var existing = await _repository.GetByIdAsync(request.VehicleId);
            if (existing == null)
                throw new InvalidOperationException($"Không tìm thấy xe {request.VehicleId}");

            // Check duplicate license plate - single query
            if (await _repository.IsLicensePlateExistsAsync(
                request.LicensePlate,
                request.VehicleId,
                cancellationToken))
            {
                throw new InvalidOperationException($"Biển số xe '{request.LicensePlate}' đã được sử dụng");
            }

            // Check duplicate VIN - single query
            if (!string.IsNullOrEmpty(request.Vin) &&
                await _repository.IsVinExistsAsync(request.Vin, request.VehicleId, cancellationToken))
            {
                throw new InvalidOperationException($"VIN '{request.Vin}' đã được sử dụng");
            }

            existing.CustomerId = request.CustomerId;
            existing.ModelId = request.ModelId;
            existing.LicensePlate = request.LicensePlate.ToUpper();
            existing.Vin = request.Vin?.ToUpper();
            existing.Color = request.Color;
            existing.PurchaseDate = request.PurchaseDate;
            existing.Mileage = request.Mileage;
            existing.LastMaintenanceDate = request.LastMaintenanceDate;
            existing.NextMaintenanceDate = request.NextMaintenanceDate;
            existing.LastMaintenanceMileage = request.LastMaintenanceMileage;
            existing.NextMaintenanceMileage = request.NextMaintenanceMileage;
            existing.BatteryHealthPercent = request.BatteryHealthPercent;
            existing.VehicleCondition = request.VehicleCondition;
            existing.InsuranceNumber = request.InsuranceNumber;
            existing.InsuranceExpiry = request.InsuranceExpiry;
            existing.RegistrationExpiry = request.RegistrationExpiry;
            existing.Notes = request.Notes;
            existing.IsActive = request.IsActive;
            existing.UpdatedDate = DateTime.UtcNow;
            existing.UpdatedBy = updatedByUserId;

            await _repository.UpdateAsync(existing);

            var updated = await _repository.GetByIdWithDetailsAsync(request.VehicleId, cancellationToken);

            _logger.LogInformation("Vehicle updated: {VehicleId}", request.VehicleId);
            InvalidateVehicleCache(request.VehicleId);

            return MapToDto(updated!);
        }

        public async Task<bool> DeleteAsync(
            int vehicleId,
            CancellationToken cancellationToken = default)
        {
            if (!await _repository.CanDeleteAsync(vehicleId, cancellationToken))
            {
                throw new InvalidOperationException("Không thể xóa xe có lịch hẹn hoặc phiếu công việc");
            }

            var result = await _repository.DeleteAsync(vehicleId);
            if (result)
            {
                InvalidateVehicleCache(vehicleId);
            }

            return result;
        }

        public Task<bool> CanDeleteAsync(int vehicleId, CancellationToken cancellationToken = default)
        {
            return _repository.CanDeleteAsync(vehicleId, cancellationToken);
        }

        public async Task<bool> UpdateMileageAsync(
            int vehicleId,
            int newMileage,
            int updatedByUserId,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await _repository.GetByIdAsync(vehicleId);
            if (vehicle == null)
                return false;

            if (newMileage < (vehicle.Mileage ?? 0))
            {
                throw new InvalidOperationException("Số km mới không thể nhỏ hơn số km hiện tại");
            }

            vehicle.Mileage = newMileage;
            vehicle.UpdatedDate = DateTime.UtcNow;
            vehicle.UpdatedBy = updatedByUserId;

            await _repository.UpdateAsync(vehicle);
            InvalidateVehicleCache(vehicleId);

            return true;
        }

        private CustomerVehicleResponseDto MapToDto(CustomerVehicle vehicle)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            return new CustomerVehicleResponseDto
            {
                VehicleId = vehicle.VehicleId,
                CustomerId = vehicle.CustomerId,
                CustomerName = vehicle.Customer?.FullName ?? string.Empty,
                CustomerCode = vehicle.Customer?.CustomerCode ?? string.Empty,
                ModelId = vehicle.ModelId,
                ModelName = vehicle.Model?.ModelName ?? string.Empty,
                BrandId = vehicle.Model?.BrandId ?? 0,
                BrandName = vehicle.Model?.Brand?.BrandName ?? string.Empty,
                FullModelName = $"{vehicle.Model?.Brand?.BrandName} {vehicle.Model?.ModelName}",
                LicensePlate = vehicle.LicensePlate,
                Vin = vehicle.Vin,
                Color = vehicle.Color,
                PurchaseDate = vehicle.PurchaseDate,
                Mileage = vehicle.Mileage,
                LastMaintenanceDate = vehicle.LastMaintenanceDate,
                NextMaintenanceDate = vehicle.NextMaintenanceDate,
                LastMaintenanceMileage = vehicle.LastMaintenanceMileage,
                NextMaintenanceMileage = vehicle.NextMaintenanceMileage,
                BatteryHealthPercent = vehicle.BatteryHealthPercent,
                VehicleCondition = vehicle.VehicleCondition,
                InsuranceNumber = vehicle.InsuranceNumber,
                InsuranceExpiry = vehicle.InsuranceExpiry,
                RegistrationExpiry = vehicle.RegistrationExpiry,
                Notes = vehicle.Notes,
                IsActive = vehicle.IsActive ?? false,
                CreatedDate = vehicle.CreatedDate ?? DateTime.UtcNow,
                UpdatedDate = vehicle.UpdatedDate,

                // Computed properties
                IsMaintenanceDue = IsMaintenanceDue(vehicle, today),
                IsInsuranceExpiring = vehicle.InsuranceExpiry.HasValue &&
                    vehicle.InsuranceExpiry.Value <= today.AddDays(30),
                IsRegistrationExpiring = vehicle.RegistrationExpiry.HasValue &&
                    vehicle.RegistrationExpiry.Value <= today.AddDays(30),
                DaysSinceLastMaintenance = vehicle.LastMaintenanceDate.HasValue
                    ? today.DayNumber - vehicle.LastMaintenanceDate.Value.DayNumber
                    : null,
                DaysUntilNextMaintenance = vehicle.NextMaintenanceDate.HasValue
                    ? vehicle.NextMaintenanceDate.Value.DayNumber - today.DayNumber
                    : null,
                MaintenanceStatus = GetMaintenanceStatus(vehicle, today)
            };
        }

        private static bool IsMaintenanceDue(CustomerVehicle vehicle, DateOnly today)
        {
            if (vehicle.NextMaintenanceDate.HasValue && vehicle.NextMaintenanceDate <= today)
                return true;

            if (vehicle.NextMaintenanceMileage.HasValue &&
                vehicle.Mileage.HasValue &&
                vehicle.Mileage >= vehicle.NextMaintenanceMileage)
                return true;

            return false;
        }

        private static string GetMaintenanceStatus(CustomerVehicle vehicle, DateOnly today)
        {
            if (!vehicle.NextMaintenanceDate.HasValue && !vehicle.NextMaintenanceMileage.HasValue)
                return "Chưa lên lịch";

            if (IsMaintenanceDue(vehicle, today))
                return "Cần bảo dưỡng";

            if (vehicle.NextMaintenanceDate.HasValue)
            {
                var daysUntil = vehicle.NextMaintenanceDate.Value.DayNumber - today.DayNumber;
                if (daysUntil <= 7)
                    return "Sắp tới hạn";
                if (daysUntil <= 30)
                    return "Bảo dưỡng trong tháng";
            }

            return "Bình thường";
        }

        private void InvalidateVehicleCache(int vehicleId)
        {
            _cache.Remove($"Vehicle_{vehicleId}");
            InvalidateListCaches();
        }

        private void InvalidateListCaches()
        {
            _cache.Remove("Vehicles_MaintenanceDue");
        }
    }
}