namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;

/// <summary>
/// Lightweight work order summary for list views
/// Optimized for performance with minimal data
/// </summary>
public class WorkOrderSummaryDto
{
    public int WorkOrderId { get; set; }
    public string WorkOrderCode { get; set; } = null!;

    // Customer and Vehicle (minimal info)
    public string CustomerName { get; set; } = null!;
    public string VehiclePlate { get; set; } = null!;
    public string VehicleModel { get; set; } = null!;

    // Service Center
    public string ServiceCenterName { get; set; } = null!;

    // Status
    public int StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? StatusColor { get; set; }

    // Priority
    public string? Priority { get; set; }

    // Source Type
    public string? SourceType { get; set; }

    // Dates
    public DateTime? StartDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public DateTime CreatedDate { get; set; }

    // Technician
    public string? TechnicianName { get; set; }

    // Progress
    public decimal? ProgressPercentage { get; set; }

    // Financial (summary)
    public decimal? FinalAmount { get; set; }

    // Flags
    public bool RequiresApproval { get; set; }
    public bool QualityCheckRequired { get; set; }
}
