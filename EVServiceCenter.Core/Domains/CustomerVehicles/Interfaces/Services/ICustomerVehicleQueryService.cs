using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;

namespace EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services
{
    public interface ICustomerVehicleQueryService
    {
        Task<PagedResult<CustomerVehicleResponseDto>> GetAllAsync(
            CustomerVehicleQueryDto query,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CustomerVehicleResponseDto>> GetVehiclesByCustomerAsync(
            int customerId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CustomerVehicleResponseDto>> GetVehiclesByModelAsync(
            int modelId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CustomerVehicleResponseDto>> GetMaintenanceDueVehiclesAsync(
            CancellationToken cancellationToken = default);

        Task<CustomerVehicleResponseDto?> GetByLicensePlateAsync(
            string licensePlate,
            CancellationToken cancellationToken = default);
    }
}