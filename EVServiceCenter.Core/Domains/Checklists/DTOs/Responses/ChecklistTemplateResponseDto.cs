namespace EVServiceCenter.Core.Domains.Checklists.DTOs.Responses;

/// <summary>
/// Response DTO for checklist template details
/// </summary>
public class ChecklistTemplateResponseDto
{
    public int TemplateId { get; set; }
    public string TemplateName { get; set; } = null!;
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }

    /// <summary>
    /// Total number of items in this template
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Template items parsed from JSON
    /// </summary>
    public List<ChecklistTemplateItemResponseDto> Items { get; set; } = new();
}

/// <summary>
/// Individual template item in response
/// </summary>
public class ChecklistTemplateItemResponseDto
{
    public int Order { get; set; }
    public string Description { get; set; } = null!;
    public bool IsRequired { get; set; }
}
