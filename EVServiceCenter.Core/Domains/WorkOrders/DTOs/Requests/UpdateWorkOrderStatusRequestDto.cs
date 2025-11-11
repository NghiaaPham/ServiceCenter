using System.ComponentModel.DataAnnotations;

namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;

/// <summary>
/// Request DTO for updating work order status
/// </summary>
public class UpdateWorkOrderStatusRequestDto
{
    /// <summary>
    /// New status ID
    /// Common statuses: Pending, In Progress, Waiting for Parts, Waiting for Approval,
    /// Quality Check, Completed, Cancelled
    /// </summary>
    [Required(ErrorMessage = "Status ID is required")]
    public int StatusId { get; set; }

    /// <summary>
    /// Notes about the status change
    /// </summary>
    [StringLength(500)]
    public string? Notes { get; set; }
}
