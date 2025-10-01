using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.CustomerVehicles.Services
{
    public class CustomerVehicleStatisticsService : ICustomerVehicleStatisticsService
    {
        private readonly ICustomerVehicleRepository _repository;
        private readonly ICustomerVehicleStatisticsRepository _statsRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CustomerVehicleStatisticsService> _logger;

        public CustomerVehicleStatisticsService(
            ICustomerVehicleRepository repository,
            ICustomerVehicleStatisticsRepository statsRepository,
            IMemoryCache cache,
            ILogger<CustomerVehicleStatisticsService> logger)
        {
            _repository = repository;
            _statsRepository = statsRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<object> GetVehicleStatisticsAsync(
            int vehicleId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"VehicleStats_{vehicleId}";
            if (_cache.TryGetValue<object>(cacheKey, out var cached))
                return cached;

            var vehicle = await _repository.GetByIdWithDetailsAsync(vehicleId, cancellationToken);
            if (vehicle == null)
                throw new InvalidOperationException($"Không tìm thấy xe {vehicleId}");

            var vehicleStats = await _statsRepository.GetBatchStatisticsAsync(
                new[] { vehicleId },
                cancellationToken);

            var statsData = vehicleStats.GetValueOrDefault(vehicleId, new Core.Domains.CustomerVehicles.DTOs.VehicleStatistics());

            var stats = new
            {
                VehicleId = vehicle.VehicleId,
                LicensePlate = vehicle.LicensePlate,
                CustomerName = vehicle.Customer?.FullName,
                FullModelName = $"{vehicle.Model?.Brand?.BrandName} {vehicle.Model?.ModelName}",
                TotalWorkOrders = statsData.TotalWorkOrders,
                TotalMaintenanceRecords = statsData.TotalMaintenanceRecords,
                TotalSpentOnVehicle = statsData.TotalSpentOnVehicle,
                CurrentMileage = vehicle.Mileage,
                AverageSpentPerWorkOrder = statsData.TotalWorkOrders > 0
                    ? Math.Round(statsData.TotalSpentOnVehicle / statsData.TotalWorkOrders, 2)
                    : 0,
                IsActive = vehicle.IsActive ?? false,
                LastUpdated = DateTime.UtcNow
            };

            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));

            return stats;
        }

        public async Task<object> GetAllVehiclesStatisticsAsync(
            CancellationToken cancellationToken = default)
        {
            var cacheKey = "Vehicles_AllStats";
            if (_cache.TryGetValue<object>(cacheKey, out var cached))
                return cached;

            var vehicles = await _repository.GetQueryable()
                .Include(v => v.Customer)
                .Include(v => v.Model)
                    .ThenInclude(m => m.Brand)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var today = DateOnly.FromDateTime(DateTime.Today);

            var stats = new
            {
                TotalVehicles = vehicles.Count,
                ActiveVehicles = vehicles.Count(v => v.IsActive == true),
                InactiveVehicles = vehicles.Count(v => v.IsActive == false),
                MaintenanceDueVehicles = vehicles.Count(v =>
                    (v.NextMaintenanceDate.HasValue && v.NextMaintenanceDate <= today) ||
                    (v.NextMaintenanceMileage.HasValue && v.Mileage.HasValue && v.Mileage >= v.NextMaintenanceMileage)),
                InsuranceExpiringVehicles = vehicles.Count(v =>
                    v.InsuranceExpiry.HasValue && v.InsuranceExpiry <= today.AddDays(30)),
                VehiclesByBrand = vehicles
                    .GroupBy(v => v.Model.Brand.BrandName)
                    .Select(g => new
                    {
                        BrandName = g.Key,
                        Count = g.Count(),
                        ActiveCount = g.Count(v => v.IsActive == true)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                AverageMileage = vehicles.Where(v => v.Mileage.HasValue).Average(v => v.Mileage),
                LastUpdated = DateTime.UtcNow
            };

            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));

            return stats;
        }

        public async Task<object> GetCustomerVehiclesStatisticsAsync(
            int customerId,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = $"CustomerVehicles_Stats_{customerId}";
            if (_cache.TryGetValue<object>(cacheKey, out var cached))
                return cached;

            var vehicles = await _repository.GetVehiclesByCustomerAsync(customerId, cancellationToken);

            if (!vehicles.Any())
                throw new InvalidOperationException($"Không tìm thấy xe nào của khách hàng {customerId}");

            var vehicleIds = vehicles.Select(v => v.VehicleId).ToList();
            var statsData = await _statsRepository.GetBatchStatisticsAsync(vehicleIds, cancellationToken);

            var stats = new
            {
                CustomerId = customerId,
                CustomerName = vehicles.First().Customer?.FullName,
                TotalVehicles = vehicles.Count(),
                ActiveVehicles = vehicles.Count(v => v.IsActive == true),
                InactiveVehicles = vehicles.Count(v => v.IsActive == false),
                TotalWorkOrdersAcrossVehicles = statsData.Sum(s => s.Value.TotalWorkOrders),
                TotalSpentAcrossVehicles = statsData.Sum(s => s.Value.TotalSpentOnVehicle),
                VehicleBreakdown = vehicles.Select(v => new
                {
                    VehicleId = v.VehicleId,
                    LicensePlate = v.LicensePlate,
                    FullModelName = $"{v.Model.Brand.BrandName} {v.Model.ModelName}",
                    IsActive = v.IsActive,
                    TotalWorkOrders = statsData.GetValueOrDefault(v.VehicleId)?.TotalWorkOrders ?? 0,
                    TotalSpent = statsData.GetValueOrDefault(v.VehicleId)?.TotalSpentOnVehicle ?? 0
                })
                .OrderByDescending(x => x.TotalSpent)
                .ToList(),
                LastUpdated = DateTime.UtcNow
            };

            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));

            return stats;
        }
    }
}