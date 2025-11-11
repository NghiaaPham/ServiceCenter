using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;

/// <summary>
/// Request DTO for adding timeline event to work order
/// </summary>
public class AddWorkOrderTimelineRequestDto
{
    /// <summary>
    /// Event type: StatusChange, ServiceAdded, PartAdded, NoteAdded,
    /// PhotoAdded, ApprovalRequested, QualityCheckStarted, etc.
    /// </summary>
    [Required(ErrorMessage = "Event type is required")]
    [StringLength(50)]
    public string EventType { get; set; } = null!;

    /// <summary>
    /// Human-readable event description
    /// </summary>
    [Required(ErrorMessage = "Event description is required")]
    [StringLength(500)]
    public string EventDescription { get; set; } = null!;

    /// <summary>
    /// Additional event data in JSON format (optional)
    /// </summary>
    public string? EventData { get; set; }

    /// <summary>
    /// Is this event visible to customer (default: true)
    /// </summary>
    public bool IsVisible { get; set; } = true;
}
