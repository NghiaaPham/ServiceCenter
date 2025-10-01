using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Responses;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.Shared.Interfaces;
using EVServiceCenter.Core.Entities;

namespace EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Repositories
{
    public interface ICustomerVehicleRepository : IRepository<CustomerVehicle>
    {
        IQueryable<CustomerVehicle> GetQueryable();

        Task<CustomerVehicle?> GetByIdWithDetailsAsync(
            int vehicleId,
            CancellationToken cancellationToken = default);

        Task<CustomerVehicle?> GetByLicensePlateAsync(
            string licensePlate,
            CancellationToken cancellationToken = default);

        Task<bool> IsLicensePlateExistsAsync(
            string licensePlate,
            int? excludeVehicleId = null,
            CancellationToken cancellationToken = default);

        Task<bool> IsVinExistsAsync(
            string vin,
            int? excludeVehicleId = null,
            CancellationToken cancellationToken = default);

        Task<bool> CanDeleteAsync(
            int vehicleId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CustomerVehicle>> GetVehiclesByCustomerAsync(
            int customerId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CustomerVehicle>> GetVehiclesByModelAsync(
            int modelId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<CustomerVehicle>> GetMaintenanceDueVehiclesAsync(
            CancellationToken cancellationToken = default);
    }
}