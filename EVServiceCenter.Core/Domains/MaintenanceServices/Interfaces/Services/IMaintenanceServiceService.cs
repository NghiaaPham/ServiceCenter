using EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Services
{
    public interface IMaintenanceServiceService
    {
        Task<PagedResult<MaintenanceServiceResponseDto>> GetAllAsync(
            MaintenanceServiceQueryDto query,
            CancellationToken cancellationToken = default);

        Task<MaintenanceServiceResponseDto?> GetByIdAsync(
            int serviceId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<MaintenanceServiceResponseDto>> GetActiveServicesAsync(
            CancellationToken cancellationToken = default);

        Task<IEnumerable<MaintenanceServiceResponseDto>> GetServicesByCategoryAsync(
            int categoryId,
            CancellationToken cancellationToken = default);

        Task<MaintenanceServiceResponseDto> CreateAsync(
            CreateMaintenanceServiceRequestDto request,
            CancellationToken cancellationToken = default);

        Task<MaintenanceServiceResponseDto> UpdateAsync(
            UpdateMaintenanceServiceRequestDto request,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int serviceId,
            CancellationToken cancellationToken = default);

        Task<bool> CanDeleteAsync(
            int serviceId,
            CancellationToken cancellationToken = default);
    }
}