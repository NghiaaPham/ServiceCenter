using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Response;

namespace EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services;

/// <summary>
/// Service để tính toán và ước tính trạng thái bảo dưỡng xe thông minh
/// </summary>
public interface IVehicleMaintenanceService
{
    /// <summary>
    /// Lấy trạng thái bảo dưỡng của xe với ước tính thông minh
    /// </summary>
    Task<VehicleMaintenanceStatusDto> GetVehicleMaintenanceStatusAsync(
        int vehicleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy trạng thái bảo dưỡng cho tất cả xe của khách hàng
    /// </summary>
    Task<List<VehicleMaintenanceStatusDto>> GetCustomerVehiclesMaintenanceStatusAsync(
        int customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy lịch sử bảo dưỡng của xe
    /// </summary>
    Task<List<MaintenanceHistoryItemDto>> GetVehicleMaintenanceHistoryAsync(
        int vehicleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật km hiện tại của xe (thủ công)
    /// </summary>
    Task UpdateVehicleMileageAsync(
        int vehicleId,
        decimal mileage,
        string? notes = null,
        CancellationToken cancellationToken = default);
}
