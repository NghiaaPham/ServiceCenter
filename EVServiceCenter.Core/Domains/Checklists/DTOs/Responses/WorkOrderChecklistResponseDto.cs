namespace EVServiceCenter.Core.Domains.Checklists.DTOs.Responses;

/// <summary>
/// Response DTO for work order checklist with completion status
/// </summary>
public class WorkOrderChecklistResponseDto
{
    public int WorkOrderId { get; set; }
    public string WorkOrderCode { get; set; } = null!;

    /// <summary>
    /// Total checklist items
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Completed items count
    /// </summary>
    public int CompletedItems { get; set; }

    /// <summary>
    /// Completion percentage (0-100)
    /// </summary>
    public decimal CompletionPercentage { get; set; }

    /// <summary>
    /// All checklist items with status
    /// </summary>
    public List<ChecklistItemDetailResponseDto> Items { get; set; } = new();
}

/// <summary>
/// Detailed checklist item with completion status
/// </summary>
public class ChecklistItemDetailResponseDto
{
    public int ItemId { get; set; }
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
