namespace EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;

/// <summary>
/// Request DTO for applying a checklist template to a work order
/// </summary>
public class ApplyChecklistTemplateRequestDto
{
    /// <summary>
    /// Template ID to apply
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// Optional: Override items (if you want to customize for this specific work order)
    /// </summary>
    public List<ChecklistTemplateItemDto>? CustomItems { get; set; }
}
