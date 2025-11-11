using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;

/// <summary>
/// Request DTO for updating an existing work order
/// </summary>
public class UpdateWorkOrderRequestDto
{
    /// <summary>
    /// Assigned technician
    /// </summary>
    public int? TechnicianId { get; set; }

    /// <summary>
    /// Service advisor
    /// </summary>
    public int? AdvisorId { get; set; }

    /// <summary>
    /// Supervisor overseeing the work
    /// </summary>
    public int? SupervisorId { get; set; }

    /// <summary>
    /// Work order priority: Low, Normal, High, Urgent
    /// </summary>
    [StringLength(20)]
    public string? Priority { get; set; }

    /// <summary>
    /// Estimated completion date
    /// </summary>
    public DateTime? EstimatedCompletionDate { get; set; }

    /// <summary>
    /// Internal notes (not visible to customer)
    /// </summary>
    [StringLength(1000)]
    public string? InternalNotes { get; set; }

    /// <summary>
    /// Technician notes and findings
    /// </summary>
    [StringLength(1000)]
    public string? TechnicianNotes { get; set; }
}
