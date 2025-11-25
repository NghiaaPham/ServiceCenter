using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Response;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.CustomerVehicles.Services;

/// <summary>
/// Service implementation cho Smart Maintenance Reminder.
/// Tính toán ước tính km dựa trên lịch sử bảo dưỡng.
/// </summary>
public class VehicleMaintenanceService : IVehicleMaintenanceService
{
    private readonly EVDbContext _context;
    private readonly ILogger<VehicleMaintenanceService> _logger;

    // Khoảng cách km giữa các lần bảo dưỡng định kỳ (có thể cấu hình)
    private const decimal DEFAULT_MAINTENANCE_INTERVAL_KM = 10000;

    public VehicleMaintenanceService(
        EVDbContext context,
        ILogger<VehicleMaintenanceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VehicleMaintenanceStatusDto> GetVehicleMaintenanceStatusAsync(
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        // Lấy thông tin xe
        var vehicle = await _context.CustomerVehicles
            .Include(v => v.Model)
            .ThenInclude(m => m.Brand)
            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId, cancellationToken);

        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy xe với ID {vehicleId}");
        }

        // Lấy lịch sử bảo dưỡng (2 lần gần nhất để tính km trung bình)
        var maintenanceHistory = await _context.MaintenanceHistories
            .Where(h => h.VehicleId == vehicleId)
            .OrderByDescending(h => h.ServiceDate)
            .Take(2)
            .ToListAsync(cancellationToken);

        var result = new VehicleMaintenanceStatusDto
        {
            VehicleId = vehicle.VehicleId,
            LicensePlate = vehicle.LicensePlate ?? string.Empty,
            ModelName = $"{vehicle.Model.Brand.BrandName} {vehicle.Model.ModelName}",
            HistoryCount = maintenanceHistory.Count,
            HasSufficientHistory = maintenanceHistory.Count >= 2
        };

        if (maintenanceHistory.Count == 0)
        {
            // Xe mới, chưa có lịch sử
            result.EstimatedCurrentKm = vehicle.Mileage ?? 0;
            result.LastMaintenanceKm = 0;
            result.LastMaintenanceDate = null;
            result.NextMaintenanceKm = DEFAULT_MAINTENANCE_INTERVAL_KM;
            result.AverageKmPerDay = 0;
            var rawRemainingKm = result.NextMaintenanceKm - result.EstimatedCurrentKm;
            result.RemainingKm = Math.Max(0, rawRemainingKm);
            result.ProgressPercent = (result.EstimatedCurrentKm / result.NextMaintenanceKm) * 100;

            // Dù chưa có lịch sử, vẫn cảnh báo nếu đã vượt ngưỡng
            result.Status = DetermineStatus(result.ProgressPercent);
            result.Message = GenerateMessage(result.Status, rawRemainingKm, 0);

            return result;
        }

        if (maintenanceHistory.Count == 1)
        {
            var lastMaintenance = maintenanceHistory[0];

            result.LastMaintenanceKm = lastMaintenance.Mileage ?? 0;
            result.LastMaintenanceDate = lastMaintenance.ServiceDate.ToDateTime(TimeOnly.MinValue);
            result.NextMaintenanceKm = result.LastMaintenanceKm + DEFAULT_MAINTENANCE_INTERVAL_KM;

            result.EstimatedCurrentKm = vehicle.Mileage ?? result.LastMaintenanceKm;
            result.AverageKmPerDay = 0;
            var rawRemainingKm = result.NextMaintenanceKm - result.EstimatedCurrentKm;
            result.RemainingKm = Math.Max(0, rawRemainingKm);
            result.ProgressPercent = ((result.EstimatedCurrentKm - result.LastMaintenanceKm) / DEFAULT_MAINTENANCE_INTERVAL_KM) * 100;
            result.Status = DetermineStatus(result.ProgressPercent);
            result.Message = GenerateMessage(result.Status, rawRemainingKm, 0);

            return result;
        }

        // Có đủ dữ liệu: tính toán thông minh
        var latestMaintenance = maintenanceHistory[0];
        var previousMaintenance = maintenanceHistory[1];

        var kmDiff = (latestMaintenance.Mileage ?? 0) - (previousMaintenance.Mileage ?? 0);
        var daysDiff = (latestMaintenance.ServiceDate.ToDateTime(TimeOnly.MinValue) - previousMaintenance.ServiceDate.ToDateTime(TimeOnly.MinValue)).TotalDays;

        decimal avgKmPerDay = 0;
        if (daysDiff > 0)
        {
            avgKmPerDay = (decimal)(kmDiff / daysDiff);
        }

        var daysSinceLastMaintenance = (DateTime.Now - latestMaintenance.ServiceDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
        var estimatedCurrentKm = (latestMaintenance.Mileage ?? 0) + (avgKmPerDay * (decimal)daysSinceLastMaintenance);

        var nextMaintenanceKm = (latestMaintenance.Mileage ?? 0) + DEFAULT_MAINTENANCE_INTERVAL_KM;
        var remainingKm = nextMaintenanceKm - estimatedCurrentKm;

        int estimatedDaysUntilMaintenance = 0;
        DateTime? estimatedNextMaintenanceDate = null;

        if (avgKmPerDay > 0)
        {
            estimatedDaysUntilMaintenance = (int)(remainingKm / avgKmPerDay);
            estimatedNextMaintenanceDate = DateTime.Now.AddDays(estimatedDaysUntilMaintenance);
        }

        var progressPercent = ((estimatedCurrentKm - (latestMaintenance.Mileage ?? 0)) / DEFAULT_MAINTENANCE_INTERVAL_KM) * 100;

        var status = DetermineStatus(progressPercent);
        var message = GenerateMessage(status, remainingKm, estimatedDaysUntilMaintenance);

        result.EstimatedCurrentKm = Math.Round(estimatedCurrentKm, 0);
        result.LastMaintenanceKm = latestMaintenance.Mileage ?? 0;
        result.LastMaintenanceDate = latestMaintenance.ServiceDate.ToDateTime(TimeOnly.MinValue);
        result.NextMaintenanceKm = nextMaintenanceKm;
        result.AverageKmPerDay = Math.Round(avgKmPerDay, 2);
        result.RemainingKm = Math.Max(0, Math.Round(remainingKm, 0));
        result.EstimatedDaysUntilMaintenance = estimatedDaysUntilMaintenance;
        result.EstimatedNextMaintenanceDate = estimatedNextMaintenanceDate;
        result.ProgressPercent = Math.Round(progressPercent, 2);
        result.Status = status;
        result.Message = message;

        _logger.LogInformation(
            "Calculated maintenance status for Vehicle {VehicleId}: EstimatedKm={EstimatedKm}, AvgKmPerDay={AvgKm}, Status={Status}",
            vehicleId, result.EstimatedCurrentKm, result.AverageKmPerDay, status);

        return result;
    }

    public async Task<List<VehicleMaintenanceStatusDto>> GetCustomerVehiclesMaintenanceStatusAsync(
        int customerId,
        CancellationToken cancellationToken = default)
    {
        var vehicles = await _context.CustomerVehicles
            .Where(v => v.CustomerId == customerId && v.IsActive == true)
            .Select(v => v.VehicleId)
            .ToListAsync(cancellationToken);

        var results = new List<VehicleMaintenanceStatusDto>();

        foreach (var vehicleId in vehicles)
        {
            try
            {
                var status = await GetVehicleMaintenanceStatusAsync(vehicleId, cancellationToken);
                results.Add(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting maintenance status for vehicle {VehicleId}", vehicleId);
            }
        }

        return results;
    }

    public async Task<List<MaintenanceHistoryItemDto>> GetVehicleMaintenanceHistoryAsync(
        int vehicleId,
        CancellationToken cancellationToken = default)
    {
        var history = await _context.MaintenanceHistories
            .Where(h => h.VehicleId == vehicleId)
            .OrderByDescending(h => h.ServiceDate)
            .Select(h => new MaintenanceHistoryItemDto
            {
                HistoryId = h.HistoryId,
                ServiceDate = h.ServiceDate.ToDateTime(TimeOnly.MinValue),
                MileageAtService = h.Mileage ?? 0,
                ServiceType = h.ServicesPerformed ?? "Bảo dưỡng định kỳ",
                TotalCost = h.TotalCost ?? 0,
                Notes = h.TechnicianNotes,
                WorkOrderId = h.WorkOrderId
            })
            .ToListAsync(cancellationToken);

        return history;
    }

    public async Task UpdateVehicleMileageAsync(
        int vehicleId,
        decimal mileage,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var vehicle = await _context.CustomerVehicles
            .FirstOrDefaultAsync(v => v.VehicleId == vehicleId, cancellationToken);

        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy xe với ID {vehicleId}");
        }

        if (vehicle.Mileage.HasValue && mileage < vehicle.Mileage.Value)
        {
            throw new InvalidOperationException(
                $"Current mileage {mileage:N0} km cannot be less than existing mileage {vehicle.Mileage.Value:N0} km");
        }

        vehicle.Mileage = (int)mileage;
        vehicle.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Updated mileage for Vehicle {VehicleId} to {Mileage} km. Notes: {Notes}",
            vehicleId, mileage, notes);
    }

    /// <summary>
    /// Xác định trạng thái dựa trên phần trăm tiến độ
    /// </summary>
    private string DetermineStatus(decimal progressPercent)
    {
        if (progressPercent >= 90)
        {
            return "Urgent";
        }
        else if (progressPercent >= 70)
        {
            return "NeedAttention";
        }
        else
        {
            return "Normal";
        }
    }

    /// <summary>
    /// Tạo message phù hợp với trạng thái
    /// </summary>
    private string GenerateMessage(string status, decimal remainingKm, int daysUntilMaintenance)
    {
        if (remainingKm < 0)
        {
            var overdueKm = Math.Abs(remainingKm);
            return status switch
            {
                "Urgent" => $"Xe cua ban da qua han bao duong khoang {overdueKm:N0} km. Vui long dat lich ngay.",
                "NeedAttention" => $"Xe da qua han khoang {overdueKm:N0} km. Nen dat lich bao duong som.",
                _ => $"Xe da vuot moc bao duong khoang {overdueKm:N0} km. Vui long dat lich."
            };
        }

        if (daysUntilMaintenance <= 0)
        {
            return status switch
            {
                "Urgent" => $"[URGENT] Xe cua ban sap den han bao duong! Con khoang {remainingKm:N0} km. Vui long dat lich ngay.",
                "NeedAttention" => $"Xe se can bao duong sau khoang {remainingKm:N0} km. Hay chuan bi dat lich.",
                _ => $"Xe cua ban con khoang {remainingKm:N0} km toi moc bao duong."
            };
        }

        return status switch
        {
            "Urgent" => $"[URGENT] Xe cua ban sap den han bao duong! Can khoang {remainingKm:N0} km hoac {daysUntilMaintenance} ngay nua. Vui long dat lich ngay.",
            "NeedAttention" => $"Xe cua ban se can bao duong sau khoang {remainingKm:N0} km hoac {daysUntilMaintenance} ngay. Hay chuan bi dat lich som.",
            _ => $"Xe cua ban van trong tinh trang tot. Can {remainingKm:N0} km hoac khoang {daysUntilMaintenance} ngay den lan bao duong tiep theo."
        };
    }
}
