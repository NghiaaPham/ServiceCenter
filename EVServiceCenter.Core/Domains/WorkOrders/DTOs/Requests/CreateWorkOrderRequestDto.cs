using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;

/// <summary>
/// Request DTO for creating a new work order
/// </summary>
public class CreateWorkOrderRequestDto
{
    /// <summary>
    /// Related appointment ID (optional - can create work order without appointment)
    /// </summary>
    public int? AppointmentId { get; set; }

    /// <summary>
    /// Customer ID who owns the vehicle
    /// </summary>
    [Required(ErrorMessage = "Customer ID is required")]
    public int CustomerId { get; set; }

    /// <summary>
    /// Vehicle ID to be serviced
    /// </summary>
    [Required(ErrorMessage = "Vehicle ID is required")]
    public int VehicleId { get; set; }

    /// <summary>
    /// Service center where work will be performed
    /// </summary>
    [Required(ErrorMessage = "Service Center ID is required")]
    public int ServiceCenterId { get; set; }

    /// <summary>
    /// Assigned technician (optional - can be assigned later)
    /// </summary>
    public int? TechnicianId { get; set; }

    /// <summary>
    /// Service advisor handling the work order
    /// </summary>
    public int? AdvisorId { get; set; }

    /// <summary>
    /// Work order priority: Low, Normal, High, Urgent
    /// </summary>
    [StringLength(20)]
    public string Priority { get; set; } = "Normal";

    /// <summary>
    /// Km thực tế khách khai báo khi tạo work order/check-in (tùy chọn)
    /// </summary>
    public int? ActualMileage { get; set; }

    /// <summary>
    /// Estimated completion date
    /// </summary>
    public DateTime? EstimatedCompletionDate { get; set; }

    /// <summary>
    /// List of service IDs to be performed
    /// </summary>
    public List<int> ServiceIds { get; set; } = new();

    /// <summary>
    /// Customer notes and requests
    /// </summary>
    [StringLength(1000)]
    public string? CustomerNotes { get; set; }

    /// <summary>
    /// Internal notes (not visible to customer)
    /// </summary>
    [StringLength(1000)]
    public string? InternalNotes { get; set; }

    /// <summary>
    /// Requires approval before starting work
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// Requires quality check after completion
    /// </summary>
    public bool QualityCheckRequired { get; set; } = true;
}
