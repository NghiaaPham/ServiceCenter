namespace EVServiceCenter.Core.Domains.Checklists.DTOs.Requests;

/// <summary>
/// Query parameters for checklist template filtering and pagination
/// </summary>
public class ChecklistTemplateQueryDto
{
    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size (default: 20, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Search by template name
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Filter by service ID
    /// </summary>
    public int? ServiceId { get; set; }

    /// <summary>
    /// Filter by category ID
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Filter by active status
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Sort by field (templateName, createdDate)
    /// </summary>
    public string SortBy { get; set; } = "templateName";

    /// <summary>
    /// Sort direction (asc, desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
}
