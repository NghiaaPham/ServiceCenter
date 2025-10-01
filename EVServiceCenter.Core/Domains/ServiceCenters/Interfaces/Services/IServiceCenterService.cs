using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceCenters.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services
{
    public interface IServiceCenterService
    {
        // CRUD Operations
        Task<ServiceCenterResponseDto?> GetByIdAsync(
            int centerId,
            CancellationToken cancellationToken = default);

        Task<ServiceCenterResponseDto?> GetByCenterCodeAsync(
            string centerCode,
            CancellationToken cancellationToken = default);

        Task<ServiceCenterResponseDto> CreateAsync(
            CreateServiceCenterRequestDto request,
            CancellationToken cancellationToken = default);

        Task<ServiceCenterResponseDto> UpdateAsync(
            UpdateServiceCenterRequestDto request,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int centerId,
            CancellationToken cancellationToken = default);

        // Validation
        Task<bool> CanDeleteAsync(
            int centerId,
            CancellationToken cancellationToken = default);

        Task<bool> IsCenterCodeExistsAsync(
            string centerCode,
            int? excludeCenterId = null,
            CancellationToken cancellationToken = default);
    }
}