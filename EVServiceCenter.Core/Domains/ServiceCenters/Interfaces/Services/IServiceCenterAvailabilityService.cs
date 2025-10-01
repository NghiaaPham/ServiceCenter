using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services
{
    public interface IServiceCenterAvailabilityService
    {
        Task<ServiceCenterAvailabilityDto?> GetAvailabilityAsync(
            int centerId,
            DateTime date,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ServiceCenterAvailabilityDto>> GetAvailableCentersAsync(
            DateTime date,
            CancellationToken cancellationToken = default);
    }
}