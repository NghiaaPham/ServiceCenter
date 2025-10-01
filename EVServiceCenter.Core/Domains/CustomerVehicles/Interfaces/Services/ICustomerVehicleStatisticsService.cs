namespace EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services
{
    public interface ICustomerVehicleStatisticsService
    {
        Task<object> GetVehicleStatisticsAsync(
            int vehicleId,
            CancellationToken cancellationToken = default);

        Task<object> GetAllVehiclesStatisticsAsync(
            CancellationToken cancellationToken = default);

        Task<object> GetCustomerVehiclesStatisticsAsync(
            int customerId,
            CancellationToken cancellationToken = default);
    }
}