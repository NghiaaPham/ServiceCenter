namespace EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Services
{
    public interface IServiceCenterStatisticsService
    {
        Task<object> GetCenterStatisticsAsync(
            int centerId,
            CancellationToken cancellationToken = default);

        Task<object> GetAllCentersStatisticsAsync(
            CancellationToken cancellationToken = default);
    }
}