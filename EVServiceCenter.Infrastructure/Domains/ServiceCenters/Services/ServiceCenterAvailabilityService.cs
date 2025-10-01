using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Responses;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.ServiceCenters.Services
{
    public class ServiceCenterAvailabilityService : IServiceCenterAvailabilityService
    {
        private readonly IServiceCenterRepository _repository;
        private readonly IServiceCenterAvailabilityRepository _availabilityRepository;
        private readonly ILogger<ServiceCenterAvailabilityService> _logger;

        public ServiceCenterAvailabilityService(
            IServiceCenterRepository repository,
            IServiceCenterAvailabilityRepository availabilityRepository,
            ILogger<ServiceCenterAvailabilityService> logger)
        {
            _repository = repository;
            _availabilityRepository = availabilityRepository;
            _logger = logger;
        }

        public async Task<ServiceCenterAvailabilityDto?> GetAvailabilityAsync(
            int centerId,
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            var center = await _repository.GetByIdAsync(centerId);
            if (center == null)
                return null;

            var bookings = await _availabilityRepository.GetDailyBookingCountAsync(
                centerId,
                date,
                cancellationToken);

            var capacity = center.Capacity ?? 0;
            var availableSlots = Math.Max(0, capacity - bookings);
            var utilizationRate = capacity > 0
                ? (decimal)bookings / capacity * 100
                : 0;

            return new ServiceCenterAvailabilityDto
            {
                CenterId = center.CenterId,
                CenterName = center.CenterName,
                Address = center.Address,
                Capacity = capacity,
                CurrentBookings = bookings,
                AvailableSlots = availableSlots,
                IsAvailable = availableSlots > 0 && (center.IsActive ?? false),
                UtilizationRate = Math.Round(utilizationRate, 2),
                Date = date.Date
            };
        }

        public async Task<IEnumerable<ServiceCenterAvailabilityDto>> GetAvailableCentersAsync(
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            var centers = await _repository.GetQueryable()
                .Where(c => c.IsActive ?? false)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (!centers.Any())
                return Enumerable.Empty<ServiceCenterAvailabilityDto>();

            var centerIds = centers.Select(c => c.CenterId).ToList();
            var bookingCounts = await _availabilityRepository.GetDailyBookingCountsAsync(
                centerIds,
                date,
                cancellationToken);

            return centers
                .Select(center =>
                {
                    var capacity = center.Capacity ?? 0;
                    var bookings = bookingCounts.GetValueOrDefault(center.CenterId, 0);
                    var available = Math.Max(0, capacity - bookings);

                    return new ServiceCenterAvailabilityDto
                    {
                        CenterId = center.CenterId,
                        CenterName = center.CenterName,
                        Address = center.Address,
                        Capacity = capacity,
                        CurrentBookings = bookings,
                        AvailableSlots = available,
                        IsAvailable = available > 0 && (center.IsActive ?? false),
                        UtilizationRate = capacity > 0
                            ? Math.Round((decimal)bookings / capacity * 100, 2)
                            : 0,
                        Date = date.Date
                    };
                })
                .Where(a => a.IsAvailable)
                .OrderByDescending(a => a.AvailableSlots)
                .ToList();
        }
    }
}
