namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;

/// <summary>
/// Response DTO for quality check operation
/// </summary>
public class QualityCheckResponseDto
{
    public int WorkOrderId { get; set; }
    public string WorkOrderCode { get; set; } = string.Empty;
    public int QualityCheckedBy { get; set; }
    public string QualityCheckedByName { get; set; } = string.Empty;
    public DateTime QualityCheckDate { get; set; }
    public int QualityRating { get; set; }
    public int? SupervisorId { get; set; }
    public string? SupervisorName { get; set; }
    public string? Message { get; set; }
}
