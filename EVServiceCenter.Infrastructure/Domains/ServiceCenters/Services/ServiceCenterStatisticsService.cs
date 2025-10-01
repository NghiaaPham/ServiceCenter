using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.ServiceCenters.Services
{
    public class ServiceCenterStatisticsService : IServiceCenterStatisticsService
    {
        private readonly IServiceCenterRepository _repository;
        private readonly IServiceCenterStatisticsRepository _statisticsRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ServiceCenterStatisticsService> _logger;

        public ServiceCenterStatisticsService(
            IServiceCenterRepository repository,
            IServiceCenterStatisticsRepository statisticsRepository,
            IMemoryCache cache,
            ILogger<ServiceCenterStatisticsService> logger)
        {
            _repository = repository;
            _statisticsRepository = statisticsRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<object> GetCenterStatisticsAsync(
      int centerId,
      CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.GetStatsKey(centerId);
            if (_cache.TryGetValue<object>(cacheKey, out var cached))
                return cached;

            var center = await _repository.GetByIdAsync(centerId);
            if (center == null)
                throw new InvalidOperationException($"Không tìm thấy trung tâm {centerId}");

            var centerIds = new[] { centerId };
            var now = DateTime.UtcNow;

            // Chạy tuần tự để tránh lỗi DbContext concurrency
            var appointmentCounts = await _statisticsRepository.GetAppointmentCountsAsync(centerIds, cancellationToken);
            var workOrderCounts = await _statisticsRepository.GetWorkOrderCountsAsync(centerIds, cancellationToken);
            var technicianCounts = await _statisticsRepository.GetTechnicianCountsAsync(centerIds, cancellationToken);
            var departmentCounts = await _statisticsRepository.GetDepartmentCountsAsync(centerIds, cancellationToken);
            var averageRatings = await _statisticsRepository.GetAverageRatingsAsync(centerIds, cancellationToken);
            var monthlyRevenues = await _statisticsRepository.GetMonthlyRevenuesAsync(centerIds, now.Year, now.Month, cancellationToken);

            var stats = new
            {
                CenterId = center.CenterId,
                CenterName = center.CenterName,
                TotalAppointments = appointmentCounts.GetValueOrDefault(centerId, 0),
                TotalWorkOrders = workOrderCounts.GetValueOrDefault(centerId, 0),
                TotalTechnicians = technicianCounts.GetValueOrDefault(centerId, 0),
                TotalDepartments = departmentCounts.GetValueOrDefault(centerId, 0),
                AverageRating = Math.Round(averageRatings.GetValueOrDefault(centerId, 0m), 2),
                MonthlyRevenue = monthlyRevenues.GetValueOrDefault(centerId, 0m),
                LastUpdated = DateTime.UtcNow
            };

            _cache.Set(cacheKey, stats, TimeSpan.FromMinutes(10));

            return stats;
        }

        public async Task<object> GetAllCentersStatisticsAsync(
            CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue<object>(CacheKeys.SERVICE_CENTER_ALL_STATS, out var cached))
                return cached;

            var centers = await _repository.GetQueryable()
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var stats = new
            {
                TotalCenters = centers.Count,
                ActiveCenters = centers.Count(c => c.IsActive == true),
                TotalCapacity = centers.Sum(c => c.Capacity),
                AverageCapacity = centers.Any() ? Math.Round(Convert.ToDecimal(centers.Average(c => c.Capacity)), 2) : 0,
                CentersByProvince = centers
                    .GroupBy(c => c.Province ?? "Không xác định")
                    .Select(g => new
                    {
                        Province = g.Key,
                        Count = g.Count(),
                        TotalCapacity = g.Sum(c => c.Capacity)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                LastUpdated = DateTime.UtcNow
            };

            _cache.Set(CacheKeys.SERVICE_CENTER_ALL_STATS, stats, TimeSpan.FromMinutes(10));

            return stats;
        }
    }
}