using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.WorkOrders.Interfaces;

/// <summary>
/// Service interface for vehicle health tracking
/// Manages EV-specific health metrics
/// </summary>
public interface IVehicleHealthService
{
    /// <summary>
    /// Record vehicle health metrics
    /// Auto-calculates overall condition if not provided
    /// </summary>
    Task<VehicleHealthMetricResponseDto> RecordHealthMetricAsync(
        RecordVehicleHealthMetricRequestDto request,
        int recordedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get latest health metrics for vehicle
    /// </summary>
    Task<VehicleHealthMetricResponseDto?> GetLatestHealthMetricAsync(
        int vehicleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get health history for vehicle
    /// Ordered by date descending
    /// </summary>
    Task<List<VehicleHealthMetricResponseDto>> GetHealthHistoryAsync(
        int vehicleId,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get health metrics for specific work order
    /// </summary>
    Task<List<VehicleHealthMetricResponseDto>> GetHealthMetricsByWorkOrderAsync(
        int workOrderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get vehicles that need health check
    /// Based on NextCheckDue date
    /// </summary>
    Task<List<VehicleHealthAlertDto>> GetVehiclesNeedingHealthCheckAsync(
        int? serviceCenterId = null,
        int daysAhead = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Auto-calculate overall condition from individual metrics
    /// Weighted average: Battery 40%, Motor 30%, Brakes 15%, Tires 15%
    /// </summary>
    decimal CalculateOverallCondition(
        decimal? batteryHealth,
        decimal? motorEfficiency,
        decimal? brakeWear,
        decimal? tireWear);
}

/// <summary>
/// Alert DTO for vehicles needing health check
/// </summary>
public class VehicleHealthAlertDto
{
    public int VehicleId { get; set; }
    public string VehiclePlate { get; set; } = null!;
    public string VehicleModel { get; set; } = null!;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string? CustomerPhone { get; set; }
    public DateOnly? NextCheckDue { get; set; }
    public int DaysUntilDue { get; set; }
    public decimal? LastOverallCondition { get; set; }
    public string? HealthStatus { get; set; }
}
