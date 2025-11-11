namespace EVServiceCenter.Core.Domains.Checklists.DTOs.Responses;

/// <summary>
/// Response DTO khi complete t?t c? checklist items c?a WorkOrder
/// </summary>
public class BulkCompleteChecklistResponseDto
{
    /// <summary>
    /// WorkOrder ID
    /// </summary>
    public int WorkOrderId { get; set; }

    /// <summary>
    /// T?ng s? checklist items
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// S? items ?ã complete thành công
    /// </summary>
    public int CompletedItems { get; set; }

    /// <summary>
    /// S? items complete th?t b?i
    /// </summary>
    public int FailedItems { get; set; }

    /// <summary>
    /// Danh sách mô t? các items failed (n?u có)
    /// </summary>
    public List<string> FailedItemDescriptions { get; set; } = new();

    /// <summary>
    /// Ph?n tr?m hoàn thành (0-100)
    /// </summary>
    public decimal CompletionPercentage { get; set; }

    /// <summary>
    /// Th?i ?i?m complete
    /// </summary>
    public DateTime CompletedDate { get; set; }

    /// <summary>
    /// Ng??i th?c hi?n complete
    /// </summary>
    public int CompletedBy { get; set; }

    /// <summary>
    /// Tên ng??i complete
    /// </summary>
    public string? CompletedByName { get; set; }

    /// <summary>
    /// Notes chung cho t?t c? items
    /// </summary>
    public string? Notes { get; set; }
}
