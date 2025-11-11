namespace EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;

/// <summary>
/// Request DTO for updating an existing checklist template
/// </summary>
public class UpdateChecklistTemplateRequestDto
{
    /// <summary>
    /// Template name
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Service ID
    /// </summary>
    public int? ServiceId { get; set; }

    /// <summary>
    /// Category ID
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Updated checklist items
    /// </summary>
    public List<ChecklistTemplateItemDto>? Items { get; set; }

    /// <summary>
    /// Is active status
    /// </summary>
    public bool? IsActive { get; set; }
}
