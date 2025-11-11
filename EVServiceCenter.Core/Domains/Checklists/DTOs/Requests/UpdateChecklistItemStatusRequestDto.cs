namespace EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;

/// <summary>
/// Request DTO for updating checklist item status and details
/// </summary>
public class UpdateChecklistItemStatusRequestDto
{
    /// <summary>
    /// Is this item completed?
    /// </summary>
    public bool? IsCompleted { get; set; }

    /// <summary>
    /// Notes about the item (e.g., findings, observations)
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Optional: Image URL for evidence (e.g., photo of completed inspection)
    /// </summary>
    public string? ImageUrl { get; set; }
}
