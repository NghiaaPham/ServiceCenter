using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services
{
    public interface IServiceCenterQueryService
    {
        Task<PagedResult<ServiceCenterResponseDto>> GetAllAsync(
            ServiceCenterQueryDto query,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ServiceCenterResponseDto>> GetActiveCentersAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ServiceCenterResponseDto>> GetCentersByProvinceAsync(
            string province,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<ServiceCenterResponseDto>> SearchCentersAsync(
            string searchTerm,
            CancellationToken cancellationToken = default);
    }
}