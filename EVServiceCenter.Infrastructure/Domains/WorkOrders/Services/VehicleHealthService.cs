using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;
using EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;
using EVServiceCenter.Core.Domains.WorkOrders.Interfaces;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.WorkOrders.Services;

/// <summary>
/// Service for tracking vehicle health metrics
/// Manages EV-specific health indicators
/// </summary>
public class VehicleHealthService : IVehicleHealthService
{
    private readonly EVDbContext _context;
    private readonly ILogger<VehicleHealthService> _logger;

    public VehicleHealthService(
        EVDbContext context,
        ILogger<VehicleHealthService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VehicleHealthMetricResponseDto> RecordHealthMetricAsync(
        RecordVehicleHealthMetricRequestDto request,
        int recordedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Recording health metrics for Vehicle {VehicleId}", request.VehicleId);

        // Auto-calculate overall condition if not provided
        var overallCondition = request.OverallCondition ?? CalculateOverallCondition(
            request.BatteryHealth,
            request.MotorEfficiency,
            request.BrakeWear,
            request.TireWear);

        var metric = new VehicleHealthMetric
        {
            VehicleId = request.VehicleId,
            WorkOrderId = request.WorkOrderId,
            MetricDate = DateOnly.FromDateTime(DateTime.UtcNow),
            BatteryHealth = request.BatteryHealth,
            MotorEfficiency = request.MotorEfficiency,
            BrakeWear = request.BrakeWear,
            TireWear = request.TireWear,
            OverallCondition = overallCondition,
            DiagnosticCodes = request.DiagnosticCodes,
            Recommendations = request.Recommendations,
            NextCheckDue = request.NextCheckDue,
            CreatedBy = recordedBy,
            CreatedDate = DateTime.UtcNow
        };

        _context.VehicleHealthMetrics.Add(metric);
        await _context.SaveChangesAsync(cancellationToken);

        // Load related data for response
        var vehicle = await _context.CustomerVehicles
            .Include(v => v.Model)
            .FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId, cancellationToken);

        var workOrder = request.WorkOrderId.HasValue
            ? await _context.WorkOrders.FindAsync(new object[] { request.WorkOrderId.Value }, cancellationToken)
            : null;

        var user = await _context.Users.FindAsync(new object[] { recordedBy }, cancellationToken);

        return new VehicleHealthMetricResponseDto
        {
            MetricId = metric.MetricId,
            VehicleId = metric.VehicleId,
            VehiclePlate = vehicle?.LicensePlate ?? "",
            VehicleModel = vehicle?.Model?.ModelName ?? "",
            WorkOrderId = metric.WorkOrderId,
            WorkOrderCode = workOrder?.WorkOrderCode,
            MetricDate = metric.MetricDate,
            BatteryHealth = metric.BatteryHealth,
            MotorEfficiency = metric.MotorEfficiency,
            BrakeWear = metric.BrakeWear,
            TireWear = metric.TireWear,
            OverallCondition = metric.OverallCondition,
            DiagnosticCodes = metric.DiagnosticCodes,
            Recommendations = metric.Recommendations,
            NextCheckDue = metric.NextCheckDue,
            CreatedBy = recordedBy,
            CreatedByName = user?.FullName,
            CreatedDate = metric.CreatedDate ?? DateTime.UtcNow
        };
    }

    public async Task<VehicleHealthMetricResponseDto?> GetLatestHealthMetricAsync(
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        var metric = await _context.VehicleHealthMetrics
            .Include(m => m.Vehicle).ThenInclude(v => v.Model)
            .Include(m => m.WorkOrder)
            .Include(m => m.CreatedByNavigation)
            .Where(m => m.VehicleId == vehicleId)
            .OrderByDescending(m => m.MetricDate)
            .ThenByDescending(m => m.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (metric == null)
            return null;

        return MapToResponseDto(metric);
    }

    public async Task<List<VehicleHealthMetricResponseDto>> GetHealthHistoryAsync(
        int vehicleId,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var metrics = await _context.VehicleHealthMetrics
            .Include(m => m.Vehicle).ThenInclude(v => v.Model)
            .Include(m => m.WorkOrder)
            .Include(m => m.CreatedByNavigation)
            .Where(m => m.VehicleId == vehicleId)
            .OrderByDescending(m => m.MetricDate)
            .ThenByDescending(m => m.CreatedDate)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return metrics.Select(MapToResponseDto).ToList();
    }

    public async Task<List<VehicleHealthMetricResponseDto>> GetHealthMetricsByWorkOrderAsync(
        int workOrderId,
        CancellationToken cancellationToken = default)
    {
        var metrics = await _context.VehicleHealthMetrics
            .Include(m => m.Vehicle).ThenInclude(v => v.Model)
            .Include(m => m.WorkOrder)
            .Include(m => m.CreatedByNavigation)
            .Where(m => m.WorkOrderId == workOrderId)
            .OrderByDescending(m => m.MetricDate)
            .ThenByDescending(m => m.CreatedDate)
            .ToListAsync(cancellationToken);

        return metrics.Select(MapToResponseDto).ToList();
    }

    public async Task<List<VehicleHealthAlertDto>> GetVehiclesNeedingHealthCheckAsync(
        int? serviceCenterId = null,
        int daysAhead = 30,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(daysAhead));

        // First, get the latest metric IDs for each vehicle
        var latestMetricIds = await (
            from metric in _context.VehicleHealthMetrics
            where metric.NextCheckDue <= cutoffDate && metric.NextCheckDue.HasValue
            group metric by metric.VehicleId into g
            select g.OrderByDescending(m => m.MetricDate)
                   .ThenByDescending(m => m.CreatedDate)
                   .Select(m => m.MetricId)
                   .FirstOrDefault()
        ).ToListAsync(cancellationToken);

        // Then load the metrics with navigation properties
        var latestMetrics = await _context.VehicleHealthMetrics
            .Include(m => m.Vehicle).ThenInclude(v => v.Model)
            .Include(m => m.Vehicle).ThenInclude(v => v.Customer)
            .Where(m => latestMetricIds.Contains(m.MetricId))
            .ToListAsync(cancellationToken);

        // Filter by service center if provided (through work orders)
        if (serviceCenterId.HasValue)
        {
            latestMetrics = latestMetrics
                .Where(m => m.WorkOrderId.HasValue)
                .ToList();

            // Further filter by checking work order's service center
            var workOrderIds = latestMetrics.Select(m => m.WorkOrderId!.Value).Distinct().ToList();
            var workOrdersInCenter = await _context.WorkOrders
                .Where(w => workOrderIds.Contains(w.WorkOrderId) && w.ServiceCenterId == serviceCenterId.Value)
                .Select(w => w.WorkOrderId)
                .ToListAsync(cancellationToken);

            latestMetrics = latestMetrics
                .Where(m => m.WorkOrderId.HasValue && workOrdersInCenter.Contains(m.WorkOrderId.Value))
                .ToList();
        }

        var alerts = latestMetrics.Select(m => new VehicleHealthAlertDto
        {
            VehicleId = m.VehicleId,
            VehiclePlate = m.Vehicle.LicensePlate,
            VehicleModel = m.Vehicle.Model?.ModelName ?? "",
            CustomerId = m.Vehicle.CustomerId,
            CustomerName = m.Vehicle.Customer?.FullName ?? "",
            CustomerPhone = m.Vehicle.Customer?.PhoneNumber,
            NextCheckDue = m.NextCheckDue,
            DaysUntilDue = m.NextCheckDue.HasValue
                ? (m.NextCheckDue.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days
                : 0,
            LastOverallCondition = m.OverallCondition,
            HealthStatus = m.OverallCondition switch
            {
                >= 80 => "Excellent",
                >= 60 => "Good",
                >= 40 => "Fair",
                >= 20 => "Poor",
                _ => "Critical"
            }
        }).ToList();

        return alerts.OrderBy(a => a.DaysUntilDue).ToList();
    }

    public decimal CalculateOverallCondition(
        decimal? batteryHealth,
        decimal? motorEfficiency,
        decimal? brakeWear,
        decimal? tireWear)
    {
        // Weighted average: Battery 40%, Motor 30%, Brakes 15%, Tires 15%
        var total = 0m;
        var weight = 0m;

        if (batteryHealth.HasValue)
        {
            total += batteryHealth.Value * 0.40m;
            weight += 0.40m;
        }

        if (motorEfficiency.HasValue)
        {
            total += motorEfficiency.Value * 0.30m;
            weight += 0.30m;
        }

        if (brakeWear.HasValue)
        {
            total += (100 - brakeWear.Value) * 0.15m; // Invert wear (lower wear = better)
            weight += 0.15m;
        }

        if (tireWear.HasValue)
        {
            total += (100 - tireWear.Value) * 0.15m; // Invert wear (lower wear = better)
            weight += 0.15m;
        }

        return weight > 0 ? Math.Round(total / weight, 2) : 0;
    }

    #region Helper Methods

    private VehicleHealthMetricResponseDto MapToResponseDto(VehicleHealthMetric metric)
    {
        return new VehicleHealthMetricResponseDto
        {
            MetricId = metric.MetricId,
            VehicleId = metric.VehicleId,
            VehiclePlate = metric.Vehicle?.LicensePlate ?? "",
            VehicleModel = metric.Vehicle?.Model?.ModelName ?? "",
            WorkOrderId = metric.WorkOrderId,
            WorkOrderCode = metric.WorkOrder?.WorkOrderCode,
            MetricDate = metric.MetricDate,
            BatteryHealth = metric.BatteryHealth,
            MotorEfficiency = metric.MotorEfficiency,
            BrakeWear = metric.BrakeWear,
            TireWear = metric.TireWear,
            OverallCondition = metric.OverallCondition,
            DiagnosticCodes = metric.DiagnosticCodes,
            Recommendations = metric.Recommendations,
            NextCheckDue = metric.NextCheckDue,
            CreatedBy = metric.CreatedBy,
            CreatedByName = metric.CreatedByNavigation?.FullName,
            CreatedDate = metric.CreatedDate ?? DateTime.UtcNow
        };
    }

    #endregion
}
