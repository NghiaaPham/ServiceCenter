using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Responses;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.CustomerVehicles.Services
{
    public class CustomerVehicleQueryService : ICustomerVehicleQueryService
    {
        private readonly ICustomerVehicleRepository _repository;
        private readonly ICustomerVehicleStatisticsRepository _statsRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CustomerVehicleQueryService> _logger;

        public CustomerVehicleQueryService(
            ICustomerVehicleRepository repository,
            ICustomerVehicleStatisticsRepository statsRepository,
            IMemoryCache cache,
            ILogger<CustomerVehicleQueryService> logger)
        {
            _repository = repository;
            _statsRepository = statsRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PagedResult<CustomerVehicleResponseDto>> GetAllAsync(
            CustomerVehicleQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var vehiclesQuery = _repository.GetQueryable();

            // Include related entities based on flags
            if (query.IncludeCustomer)
                vehiclesQuery = vehiclesQuery.Include(v => v.Customer);

            if (query.IncludeModel)
                vehiclesQuery = vehiclesQuery
                    .Include(v => v.Model)
                    .ThenInclude(m => m.Brand);

            // Apply filters
            vehiclesQuery = ApplyFilters(vehiclesQuery, query);

            // Get total count
            var totalCount = await vehiclesQuery.CountAsync(cancellationToken);

            // Apply sorting & pagination
            vehiclesQuery = ApplySorting(vehiclesQuery, query.SortBy, query.SortOrder);
            var vehicles = await vehiclesQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map to DTOs with optional stats - batch load stats
            var dtos = await MapToDtosAsync(vehicles, query.IncludeStats, cancellationToken);

            return PagedResultFactory.Create(dtos, totalCount, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<CustomerVehicleResponseDto>> GetVehiclesByCustomerAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            var vehicles = await _repository.GetVehiclesByCustomerAsync(customerId, cancellationToken);
            return vehicles.Select(MapToDto);
        }

        public async Task<IEnumerable<CustomerVehicleResponseDto>> GetVehiclesByModelAsync(
            int modelId,
            CancellationToken cancellationToken = default)
        {
            var vehicles = await _repository.GetVehiclesByModelAsync(modelId, cancellationToken);
            return vehicles.Select(MapToDto);
        }

        public async Task<IEnumerable<CustomerVehicleResponseDto>> GetMaintenanceDueVehiclesAsync(
            CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue<List<CustomerVehicleResponseDto>>("Vehicles_MaintenanceDue", out var cached))
                return cached;

            var vehicles = await _repository.GetMaintenanceDueVehiclesAsync(cancellationToken);
            var dtos = vehicles.Select(MapToDto).ToList();

            _cache.Set("Vehicles_MaintenanceDue", dtos, TimeSpan.FromMinutes(5));

            return dtos;
        }

        public async Task<CustomerVehicleResponseDto?> GetByLicensePlateAsync(
            string licensePlate,
            CancellationToken cancellationToken = default)
        {
            var vehicle = await _repository.GetByLicensePlateAsync(licensePlate, cancellationToken);
            return vehicle != null ? MapToDto(vehicle) : null;
        }

        private async Task<List<CustomerVehicleResponseDto>> MapToDtosAsync(
            List<CustomerVehicle> vehicles,
            bool includeStats,
            CancellationToken cancellationToken)
        {
            var dtos = vehicles.Select(MapToDto).ToList();

            if (includeStats && vehicles.Any())
            {
                var vehicleIds = vehicles.Select(v => v.VehicleId).ToList();

                // Batch load statistics - single query per stat type
                var stats = await _statsRepository.GetBatchStatisticsAsync(vehicleIds, cancellationToken);

                foreach (var dto in dtos)
                {
                    if (stats.TryGetValue(dto.VehicleId, out var vehicleStats))
                    {
                        dto.TotalWorkOrders = vehicleStats.TotalWorkOrders;
                        dto.TotalMaintenanceRecords = vehicleStats.TotalMaintenanceRecords;
                        dto.TotalSpentOnVehicle = vehicleStats.TotalSpentOnVehicle;
                    }
                }
            }

            return dtos;
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

        private static IQueryable<CustomerVehicle> ApplyFilters(
            IQueryable<CustomerVehicle> query,
            CustomerVehicleQueryDto filters)
        {
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var search = filters.SearchTerm.ToLower();
                query = query.Where(v =>
                    v.LicensePlate.ToLower().Contains(search) ||
                    (v.Vin != null && v.Vin.ToLower().Contains(search)));
            }

            if (filters.CustomerId.HasValue)
                query = query.Where(v => v.CustomerId == filters.CustomerId.Value);

            if (filters.ModelId.HasValue)
                query = query.Where(v => v.ModelId == filters.ModelId.Value);

            if (filters.BrandId.HasValue)
                query = query.Where(v => v.Model.BrandId == filters.BrandId.Value);

            if (filters.IsActive.HasValue)
                query = query.Where(v => v.IsActive == filters.IsActive.Value);

            if (filters.MaintenanceDue.HasValue && filters.MaintenanceDue.Value)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                query = query.Where(v =>
                    (v.NextMaintenanceDate.HasValue && v.NextMaintenanceDate <= today) ||
                    (v.NextMaintenanceMileage.HasValue && v.Mileage.HasValue && v.Mileage >= v.NextMaintenanceMileage));
            }

            if (filters.InsuranceExpiring.HasValue && filters.InsuranceExpiring.Value)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var expiryThreshold = today.AddDays(30);
                query = query.Where(v => v.InsuranceExpiry.HasValue && v.InsuranceExpiry <= expiryThreshold);
            }

            return query;
        }

        private static IQueryable<CustomerVehicle> ApplySorting(
            IQueryable<CustomerVehicle> query,
            string sortBy,
            string sortOrder)
        {
            var isDesc = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "licenseplate" => isDesc ? query.OrderByDescending(v => v.LicensePlate) : query.OrderBy(v => v.LicensePlate),
                "customername" => isDesc ? query.OrderByDescending(v => v.Customer.FullName) : query.OrderBy(v => v.Customer.FullName),
                "modelname" => isDesc ? query.OrderByDescending(v => v.Model.ModelName) : query.OrderBy(v => v.Model.ModelName),
                "brandname" => isDesc ? query.OrderByDescending(v => v.Model.Brand.BrandName) : query.OrderBy(v => v.Model.Brand.BrandName),
                "purchasedate" => isDesc ? query.OrderByDescending(v => v.PurchaseDate) : query.OrderBy(v => v.PurchaseDate),
                "mileage" => isDesc ? query.OrderByDescending(v => v.Mileage) : query.OrderBy(v => v.Mileage),
                "nextmaintenancedate" => isDesc ? query.OrderByDescending(v => v.NextMaintenanceDate) : query.OrderBy(v => v.NextMaintenanceDate),
                "createddate" => isDesc ? query.OrderByDescending(v => v.CreatedDate) : query.OrderBy(v => v.CreatedDate),
                _ => query.OrderBy(v => v.LicensePlate)
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
    }
}