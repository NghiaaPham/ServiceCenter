namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;

/// <summary>
/// Response DTO for checklist item
/// </summary>
public class ChecklistItemResponseDto
{
    public int ItemId { get; set; }
    public int WorkOrderId { get; set; }

    public int? TemplateId { get; set; }
    public int ItemOrder { get; set; }

    public string ItemDescription { get; set; } = null!;
    public bool IsRequired { get; set; }
    public bool IsCompleted { get; set; }

    public int? CompletedBy { get; set; }
    public string? CompletedByName { get; set; }
    public DateTime? CompletedDate { get; set; }

    public string? Notes { get; set; }
    public string? ImageUrl { get; set; }
}
