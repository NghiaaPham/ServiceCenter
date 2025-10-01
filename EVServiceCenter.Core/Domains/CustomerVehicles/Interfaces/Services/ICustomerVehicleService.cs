using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services
{
    public interface ICustomerVehicleService
    {
        Task<CustomerVehicleResponseDto?> GetByIdAsync(
            int vehicleId,
            CancellationToken cancellationToken = default);

        Task<CustomerVehicleResponseDto> CreateAsync(
            CreateCustomerVehicleRequestDto request,
            int createdByUserId,
            CancellationToken cancellationToken = default);

        Task<CustomerVehicleResponseDto> UpdateAsync(
            UpdateCustomerVehicleRequestDto request,
            int updatedByUserId,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteAsync(
            int vehicleId,
            CancellationToken cancellationToken = default);

        Task<bool> CanDeleteAsync(
            int vehicleId,
            CancellationToken cancellationToken = default);

        Task<bool> UpdateMileageAsync(
            int vehicleId,
            int newMileage,
            int updatedByUserId,
            CancellationToken cancellationToken = default);
    }
}