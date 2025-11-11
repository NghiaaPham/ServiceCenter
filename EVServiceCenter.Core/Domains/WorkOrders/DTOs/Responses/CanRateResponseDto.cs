namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;

/// <summary>
/// Response DTO for can-rate check (AllowAnonymous endpoint)
/// </summary>
public class CanRateResponseDto
{
    public int WorkOrderId { get; set; }
    public string WorkOrderCode { get; set; } = string.Empty;
    public bool CanRate { get; set; }
    public string? Reason { get; set; }
    public string? WorkOrderStatus { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool QualityCheckRequired { get; set; }
    public bool QualityCheckCompleted { get; set; }
    public string? QualityCheckedByName { get; set; }
    public DateTime? QualityCheckDate { get; set; }
    public int? QualityRating { get; set; }
}
