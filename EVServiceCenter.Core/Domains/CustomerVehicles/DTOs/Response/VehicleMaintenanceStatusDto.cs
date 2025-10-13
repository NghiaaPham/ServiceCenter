namespace EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Response;

/// <summary>
/// DTO trả về trạng thái bảo dưỡng của xe với ước tính thông minh
/// </summary>
public class VehicleMaintenanceStatusDto
{
    /// <summary>
    /// ID của xe
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Biển số xe
    /// </summary>
    public string LicensePlate { get; set; } = string.Empty;

    /// <summary>
    /// Tên model xe
    /// </summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>
    /// Km hiện tại (ước tính thông minh dựa trên lịch sử)
    /// </summary>
    public decimal EstimatedCurrentKm { get; set; }

    /// <summary>
    /// Km tại lần bảo dưỡng cuối cùng
    /// </summary>
    public decimal LastMaintenanceKm { get; set; }

    /// <summary>
    /// Ngày bảo dưỡng cuối cùng
    /// </summary>
    public DateTime? LastMaintenanceDate { get; set; }

    /// <summary>
    /// Km dự kiến bảo dưỡng tiếp theo
    /// </summary>
    public decimal NextMaintenanceKm { get; set; }

    /// <summary>
    /// Km trung bình mỗi ngày (tính từ lịch sử)
    /// </summary>
    public decimal AverageKmPerDay { get; set; }

    /// <summary>
    /// Km còn lại đến lần bảo dưỡng tiếp theo
    /// </summary>
    public decimal RemainingKm { get; set; }

    /// <summary>
    /// Số ngày ước tính đến lần bảo dưỡng tiếp theo
    /// </summary>
    public int EstimatedDaysUntilMaintenance { get; set; }

    /// <summary>
    /// Ngày dự kiến bảo dưỡng tiếp theo
    /// </summary>
    public DateTime? EstimatedNextMaintenanceDate { get; set; }

    /// <summary>
    /// Phần trăm tiến độ đến lần bảo dưỡng tiếp theo (0-100)
    /// </summary>
    public decimal ProgressPercent { get; set; }

    /// <summary>
    /// Trạng thái: Normal, NeedAttention, Urgent
    /// </summary>
    public string Status { get; set; } = "Normal";

    /// <summary>
    /// Thông báo cho khách hàng
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Có đủ dữ liệu lịch sử để ước tính không
    /// </summary>
    public bool HasSufficientHistory { get; set; }

    /// <summary>
    /// Số lần bảo dưỡng trong lịch sử
    /// </summary>
    public int HistoryCount { get; set; }
}
