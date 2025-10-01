using EVServiceCenter.Core.Constants;
using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Responses;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.ServiceCenters.Services
{
    public class ServiceCenterQueryService : IServiceCenterQueryService
    {
        private readonly IServiceCenterRepository _repository;
        private readonly IServiceCenterStatisticsRepository _statisticsRepository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<ServiceCenterQueryService> _logger;

        public ServiceCenterQueryService(
            IServiceCenterRepository repository,
            IServiceCenterStatisticsRepository statisticsRepository,
            IMemoryCache cache,
            ILogger<ServiceCenterQueryService> logger)
        {
            _repository = repository;
            _statisticsRepository = statisticsRepository;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PagedResult<ServiceCenterResponseDto>> GetAllAsync(
            ServiceCenterQueryDto query,
            CancellationToken cancellationToken = default)
        {
            var centersQuery = _repository.GetQueryable();

            // Apply filters
            centersQuery = ApplyFilters(centersQuery, query);

            // Get total
            var totalCount = await centersQuery.CountAsync(cancellationToken);

            // Apply sorting & pagination
            centersQuery = ApplySorting(centersQuery, query.SortBy, query.SortOrder);
            var centers = await centersQuery
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Include(c => c.Manager)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // Map with optional stats
            var dtos = await MapToDtosAsync(centers, query.IncludeStats, cancellationToken);

            return PagedResultFactory.Create(dtos, totalCount, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<ServiceCenterResponseDto>> GetActiveCentersAsync(
            CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue<List<ServiceCenterResponseDto>>(
                CacheKeys.SERVICE_CENTER_ACTIVE, out var cached))
                return cached;

            var centers = await _repository.GetQueryable()
                .Where(c => c.IsActive == true)
                .Include(c => c.Manager)
                .OrderBy(c => c.CenterName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = centers.Select(MapToDto).ToList();

            _cache.Set(CacheKeys.SERVICE_CENTER_ACTIVE, dtos, TimeSpan.FromMinutes(3));

            return dtos;
        }

        public async Task<IEnumerable<ServiceCenterResponseDto>> GetCentersByProvinceAsync(
            string province,
            CancellationToken cancellationToken = default)
        {
            var centers = await _repository.GetQueryable()
                .Where(c => c.Province == province && c.IsActive == true)
                .Include(c => c.Manager)
                .OrderBy(c => c.CenterName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return centers.Select(MapToDto);
        }

        public async Task<IEnumerable<ServiceCenterResponseDto>> SearchCentersAsync(
            string searchTerm,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetActiveCentersAsync(cancellationToken);

            var lowerSearch = searchTerm.ToLower();
            var centers = await _repository.GetQueryable()
                .Where(c =>
                    c.CenterName.ToLower().Contains(lowerSearch) ||
                    c.CenterCode.ToLower().Contains(lowerSearch) ||
                    (c.Address != null && c.Address.ToLower().Contains(lowerSearch)) ||
                    (c.Province != null && c.Province.ToLower().Contains(lowerSearch)))
                .Include(c => c.Manager)
                .OrderBy(c => c.CenterName)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return centers.Select(MapToDto);
        }

        // Helper methods
        private async Task<List<ServiceCenterResponseDto>> MapToDtosAsync(
            List<ServiceCenter> centers,
            bool includeStats,
            CancellationToken cancellationToken)
        {
            var dtos = centers.Select(MapToDto).ToList();

            if (includeStats && centers.Any())
            {
                var centerIds = centers.Select(c => c.CenterId).ToList();
                var stats = await GetBatchStatsAsync(centerIds, cancellationToken);

                foreach (var dto in dtos)
                {
                    dto.TotalAppointments = stats.AppointmentCounts.GetValueOrDefault(dto.CenterId);
                    dto.TotalWorkOrders = stats.WorkOrderCounts.GetValueOrDefault(dto.CenterId);
                    dto.TotalTechnicians = stats.TechnicianCounts.GetValueOrDefault(dto.CenterId);
                    dto.AverageRating = stats.Ratings.GetValueOrDefault(dto.CenterId);
                }
            }

            return dtos;
        }

        private async Task<BatchStats> GetBatchStatsAsync(List<int> centerIds, CancellationToken cancellationToken)
        {
            var appointmentCounts = await _statisticsRepository.GetAppointmentCountsAsync(centerIds, cancellationToken);
            var workOrderCounts = await _statisticsRepository.GetWorkOrderCountsAsync(centerIds, cancellationToken);
            var technicianCounts = await _statisticsRepository.GetTechnicianCountsAsync(centerIds, cancellationToken);
            var ratings = await _statisticsRepository.GetAverageRatingsAsync(centerIds, cancellationToken);

            return new BatchStats
            {
                AppointmentCounts = appointmentCounts,
                WorkOrderCounts = workOrderCounts,
                TechnicianCounts = technicianCounts,
                Ratings = ratings
            };
        }



        private ServiceCenterResponseDto MapToDto(ServiceCenter center)
        {
            return new ServiceCenterResponseDto
            {
                CenterId = center.CenterId,
                CenterName = center.CenterName,
                CenterCode = center.CenterCode,
                Address = center.Address ?? string.Empty,
                Ward = center.Ward,
                District = center.District,
                Province = center.Province,
                PostalCode = center.PostalCode,
                FullAddress = $"{center.Address}, {center.Ward}, {center.District}, {center.Province}".Trim(',', ' '),
                PhoneNumber = center.PhoneNumber ?? string.Empty,
                Email = center.Email,
                Website = center.Website,
                Latitude = center.Latitude,
                Longitude = center.Longitude,
                OpenTime = center.OpenTime,
                CloseTime = center.CloseTime,
                WorkingHours = $"{center.OpenTime:HH\\:mm} - {center.CloseTime:HH\\:mm}",
                Capacity = center.Capacity ?? 0,
                ManagerId = center.ManagerId,
                ManagerName = center.Manager?.FullName,
                IsActive = center.IsActive ?? false,
                Description = center.Description,
                Facilities = center.Facilities,
                ImageUrl = center.ImageUrl,
                CreatedDate = center.CreatedDate ?? DateTime.MinValue,
                UpdatedDate = center.UpdatedDate
            };
        }

        private static IQueryable<ServiceCenter> ApplyFilters(
            IQueryable<ServiceCenter> query,
            ServiceCenterQueryDto filters)
        {
            if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
            {
                var search = filters.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.CenterName.ToLower().Contains(search) ||
                    c.CenterCode.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(filters.Province))
                query = query.Where(c => c.Province == filters.Province);

            if (filters.IsActive.HasValue)
                query = query.Where(c => c.IsActive == filters.IsActive.Value);

            return query;
        }

        private static IQueryable<ServiceCenter> ApplySorting(
            IQueryable<ServiceCenter> query,
            string sortBy,
            string sortOrder)
        {
            var isDesc = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return sortBy.ToLower() switch
            {
                "centername" => isDesc ? query.OrderByDescending(c => c.CenterName) : query.OrderBy(c => c.CenterName),
                "capacity" => isDesc ? query.OrderByDescending(c => c.Capacity) : query.OrderBy(c => c.Capacity),
                _ => query.OrderBy(c => c.CenterName)
            };
        }

        private class BatchStats
        {
            public Dictionary<int, int> AppointmentCounts { get; set; } = new();
            public Dictionary<int, int> WorkOrderCounts { get; set; } = new();
            public Dictionary<int, int> TechnicianCounts { get; set; } = new();
            public Dictionary<int, decimal> Ratings { get; set; } = new();
        }
    }
}
