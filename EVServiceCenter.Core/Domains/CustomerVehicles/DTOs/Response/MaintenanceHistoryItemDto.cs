namespace EVServiceCenter.Core.Domains.CustomerVehicles.DTOs.Response;

/// <summary>
/// DTO cho từng lần bảo dưỡng trong lịch sử
/// </summary>
public class MaintenanceHistoryItemDto
{
    public int HistoryId { get; set; }
    public DateTime ServiceDate { get; set; }
    public decimal MileageAtService { get; set; }
    public string ServiceType { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }
    public int? WorkOrderId { get; set; }
}
