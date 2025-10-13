using EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Response;
using EVServiceCenter.Core.Domains.CustomerVehicles.Interfaces.Services;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.CustomerVehicles.Services;

/// <summary>
/// Service implementation cho Smart Maintenance Reminder
/// Tính toán ước tính km dựa trên lịch sử bảo dưỡng
/// </summary>
public class VehicleMaintenanceService : IVehicleMaintenanceService
{
    private readonly EVDbContext _context;
    private readonly ILogger<VehicleMaintenanceService> _logger;

    // Khoảng cách km giữa các lần bảo dưỡng định kỳ (có thể config)
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
            LicensePlate = vehicle.LicensePlate ?? "",
            ModelName = $"{vehicle.Model.Brand.BrandName} {vehicle.Model.ModelName}",
            HistoryCount = maintenanceHistory.Count,
            HasSufficientHistory = maintenanceHistory.Count >= 2
        };

        if (maintenanceHistory.Count == 0)
        {
            // Trường hợp: Xe mới, chưa có lịch sử
            result.EstimatedCurrentKm = vehicle.Mileage ?? 0;
            result.LastMaintenanceKm = 0;
            result.LastMaintenanceDate = null;
            result.NextMaintenanceKm = DEFAULT_MAINTENANCE_INTERVAL_KM;
            result.AverageKmPerDay = 0;
            result.RemainingKm = result.NextMaintenanceKm - result.EstimatedCurrentKm;
            result.ProgressPercent = (result.EstimatedCurrentKm / result.NextMaintenanceKm) * 100;
            result.Status = "Normal";
            result.Message = "Xe chưa có lịch sử bảo dưỡng. Vui lòng cập nhật km hiện tại.";

            return result;
        }

        if (maintenanceHistory.Count == 1)
        {
            // Trường hợp: Chỉ có 1 lần bảo dưỡng
            var lastMaintenance = maintenanceHistory[0];

            result.LastMaintenanceKm = lastMaintenance.Mileage ?? 0;
            result.LastMaintenanceDate = lastMaintenance.ServiceDate.ToDateTime(TimeOnly.MinValue);
            result.NextMaintenanceKm = result.LastMaintenanceKm + DEFAULT_MAINTENANCE_INTERVAL_KM;

            // Không thể tính km trung bình, dùng km hiện tại của xe
            result.EstimatedCurrentKm = vehicle.Mileage ?? result.LastMaintenanceKm;
            result.AverageKmPerDay = 0;
            result.RemainingKm = result.NextMaintenanceKm - result.EstimatedCurrentKm;
            result.ProgressPercent = ((result.EstimatedCurrentKm - result.LastMaintenanceKm) / DEFAULT_MAINTENANCE_INTERVAL_KM) * 100;
            result.Status = DetermineStatus(result.ProgressPercent);
            result.Message = "Chỉ có 1 lần bảo dưỡng. Hệ thống chưa thể ước tính chính xác.";

            return result;
        }

        // ✨ TRƯỜNG HỢP CÓ ĐỦ DỮ LIỆU: TÍNH TOÁN THÔNG MINH
        var latestMaintenance = maintenanceHistory[0];
        var previousMaintenance = maintenanceHistory[1];

        // 🔥 CÔNG THỨC 1: Tính km trung bình mỗi ngày
        var kmDiff = (latestMaintenance.Mileage ?? 0) - (previousMaintenance.Mileage ?? 0);
        var daysDiff = (latestMaintenance.ServiceDate.ToDateTime(TimeOnly.MinValue) - previousMaintenance.ServiceDate.ToDateTime(TimeOnly.MinValue)).TotalDays;

        decimal avgKmPerDay = 0;
        if (daysDiff > 0)
        {
            avgKmPerDay = (decimal)(kmDiff / daysDiff);
        }

        // 🔥 CÔNG THỨC 2: Ước tính km hiện tại
        var daysSinceLastMaintenance = (DateTime.Now - latestMaintenance.ServiceDate.ToDateTime(TimeOnly.MinValue)).TotalDays;
        var estimatedCurrentKm = (latestMaintenance.Mileage ?? 0) + (avgKmPerDay * (decimal)daysSinceLastMaintenance);

        // 🔥 CÔNG THỨC 3: Tính km còn lại và ngày dự kiến
        var nextMaintenanceKm = (latestMaintenance.Mileage ?? 0) + DEFAULT_MAINTENANCE_INTERVAL_KM;
        var remainingKm = nextMaintenanceKm - estimatedCurrentKm;

        int estimatedDaysUntilMaintenance = 0;
        DateTime? estimatedNextMaintenanceDate = null;

        if (avgKmPerDay > 0)
        {
            estimatedDaysUntilMaintenance = (int)(remainingKm / avgKmPerDay);
            estimatedNextMaintenanceDate = DateTime.Now.AddDays(estimatedDaysUntilMaintenance);
        }

        // Tính phần trăm tiến độ
        var progressPercent = ((estimatedCurrentKm - (latestMaintenance.Mileage ?? 0)) / DEFAULT_MAINTENANCE_INTERVAL_KM) * 100;

        // Xác định trạng thái
        var status = DetermineStatus(progressPercent);
        var message = GenerateMessage(status, remainingKm, estimatedDaysUntilMaintenance);

        result.EstimatedCurrentKm = Math.Round(estimatedCurrentKm, 0);
        result.LastMaintenanceKm = latestMaintenance.Mileage ?? 0;
        result.LastMaintenanceDate = latestMaintenance.ServiceDate.ToDateTime(TimeOnly.MinValue);
        result.NextMaintenanceKm = nextMaintenanceKm;
        result.AverageKmPerDay = Math.Round(avgKmPerDay, 2);
        result.RemainingKm = Math.Round(remainingKm, 0);
        result.EstimatedDaysUntilMaintenance = estimatedDaysUntilMaintenance;
        result.EstimatedNextMaintenanceDate = estimatedNextMaintenanceDate;
        result.ProgressPercent = Math.Round(progressPercent, 2);
        result.Status = status;
        result.Message = message;

        _logger.LogInformation(
            "Calculated maintenance status for Vehicle {VehicleId}: " +
            "EstimatedKm={EstimatedKm}, AvgKmPerDay={AvgKm}, Status={Status}",
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
            return "Urgent"; // ⚠️ CẬN BẢO DƯỠNG
        }
        else if (progressPercent >= 70)
        {
            return "NeedAttention"; // ⚡ CẦN CHÚ Ý
        }
        else
        {
            return "Normal"; // ✅ BÌNH THƯỜNG
        }
    }

    /// <summary>
    /// Tạo message phù hợp với trạng thái
    /// </summary>
    private string GenerateMessage(string status, decimal remainingKm, int daysUntilMaintenance)
    {
        return status switch
        {
            "Urgent" => $"⚠️ Xe của bạn sắp đến hạn bảo dưỡng! Còn khoảng {remainingKm:N0} km hoặc {daysUntilMaintenance} ngày nữa. Vui lòng đặt lịch ngay.",
            "NeedAttention" => $"⚡ Xe của bạn sẽ cần bảo dưỡng sau khoảng {remainingKm:N0} km hoặc {daysUntilMaintenance} ngày. Hãy chuẩn bị đặt lịch sớm.",
            _ => $"✅ Xe của bạn vẫn trong tình trạng tốt. Còn {remainingKm:N0} km hoặc khoảng {daysUntilMaintenance} ngày đến lần bảo dưỡng tiếp theo."
        };
    }
}
