namespace EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;

/// <summary>
/// Request DTO for creating a new checklist template
/// </summary>
public class CreateChecklistTemplateRequestDto
{
    /// <summary>
    /// Template name (e.g., "Battery Inspection Checklist")
    /// </summary>
    public string TemplateName { get; set; } = null!;

    /// <summary>
    /// Optional: Link to specific service
    /// </summary>
    public int? ServiceId { get; set; }

    /// <summary>
    /// Optional: Link to service category
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Checklist items (JSON array of item descriptions)
    /// Example: ["Check battery voltage", "Inspect terminals", "Test charging system"]
    /// </summary>
    public List<ChecklistTemplateItemDto> Items { get; set; } = new();

    /// <summary>
    /// Is this template active?
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Individual checklist template item
/// </summary>
public class ChecklistTemplateItemDto
{
    /// <summary>
    /// Display order (1, 2, 3...)
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Item description
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Is this item required for completion?
    /// </summary>
    public bool IsRequired { get; set; } = true;
}
