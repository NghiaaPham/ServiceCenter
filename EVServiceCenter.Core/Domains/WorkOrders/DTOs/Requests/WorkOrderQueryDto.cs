namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Requests;

/// <summary>
/// Query parameters for filtering and searching work orders
/// </summary>
public class WorkOrderQueryDto
{
    /// <summary>
    /// Filter by work order code (partial match)
    /// </summary>
    public string? WorkOrderCode { get; set; }

    /// <summary>
    /// Filter by customer ID
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Filter by vehicle ID
    /// </summary>
    public int? VehicleId { get; set; }

    /// <summary>
    /// Filter by service center ID
    /// </summary>
    public int? ServiceCenterId { get; set; }

    /// <summary>
    /// Filter by technician ID
    /// </summary>
    public int? TechnicianId { get; set; }

    /// <summary>
    /// Filter by status ID
    /// </summary>
    public int? StatusId { get; set; }

    /// <summary>
    /// Filter by priority: Low, Normal, High, Urgent
    /// </summary>
    public string? Priority { get; set; }

    /// <summary>
    /// Filter by start date (from)
    /// </summary>
    public DateTime? StartDateFrom { get; set; }

    /// <summary>
    /// Filter by start date (to)
    /// </summary>
    public DateTime? StartDateTo { get; set; }

    /// <summary>
    /// Filter by completion date (from)
    /// </summary>
    public DateTime? CompletedDateFrom { get; set; }

    /// <summary>
    /// Filter by completion date (to)
    /// </summary>
    public DateTime? CompletedDateTo { get; set; }

    /// <summary>
    /// Filter work orders that require approval
    /// </summary>
    public bool? RequiresApproval { get; set; }

    /// <summary>
    /// Filter work orders that require quality check
    /// </summary>
    public bool? QualityCheckRequired { get; set; }

    /// <summary>
    /// Search by customer name or vehicle plate
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (default: 20, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort by field: CreatedDate, StartDate, EstimatedCompletionDate, Priority, Status
    /// </summary>
    public string SortBy { get; set; } = "CreatedDate";

    /// <summary>
    /// Sort direction: asc or desc
    /// </summary>
    public string SortDirection { get; set; } = "desc";
}
