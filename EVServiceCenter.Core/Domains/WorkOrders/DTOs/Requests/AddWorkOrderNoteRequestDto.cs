using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;

/// <summary>
/// Request DTO for adding a note to work order timeline
/// </summary>
public class AddWorkOrderNoteRequestDto
{
    /// <summary>
    /// Note content
    /// </summary>
    [Required(ErrorMessage = "Note is required")]
    [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters")]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// Whether this note is internal (not visible to customer)
    /// </summary>
    public bool IsInternal { get; set; }
}
