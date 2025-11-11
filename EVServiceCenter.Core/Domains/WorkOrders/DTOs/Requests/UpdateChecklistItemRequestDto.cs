using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;

/// <summary>
/// Request DTO for updating checklist item status
/// </summary>
public class UpdateChecklistItemRequestDto
{
    /// <summary>
    /// Mark item as completed or not
    /// </summary>
    [Required(ErrorMessage = "IsCompleted is required")]
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Notes about the checklist item
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Image URL for proof/documentation (optional)
    /// </summary>
    [StringLength(500)]
    public string? ImageUrl { get; set; }
}
