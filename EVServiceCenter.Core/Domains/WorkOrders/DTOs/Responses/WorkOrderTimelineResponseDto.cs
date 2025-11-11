namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;

/// <summary>
/// Response DTO for work order timeline event
/// </summary>
public class WorkOrderTimelineResponseDto
{
    public int TimelineId { get; set; }
    public int WorkOrderId { get; set; }
    public string WorkOrderCode { get; set; } = null!;

    public string EventType { get; set; } = null!;
    public string EventDescription { get; set; } = null!;
    public string? EventData { get; set; }

    public DateTime EventDate { get; set; }

    public int? PerformedBy { get; set; }
    public string? PerformedByName { get; set; }

    public bool IsVisible { get; set; }
}
