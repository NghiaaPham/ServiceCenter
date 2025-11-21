namespace EVServiceCenter.Core.Domains.WorkOrders.DTOs.Responses;

/// <summary>
/// Response DTO for work order details
/// </summary>
public class WorkOrderResponseDto
{
    public int WorkOrderId { get; set; }
    public string WorkOrderCode { get; set; } = null!;

    // Related entities
    public int? AppointmentId { get; set; }
    public string? AppointmentCode { get; set; }

    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string? CustomerPhone { get; set; }

    public int VehicleId { get; set; }
    public string VehiclePlate { get; set; } = null!;
    public string VehicleModel { get; set; } = null!;

    public int ServiceCenterId { get; set; }
    public string ServiceCenterName { get; set; } = null!;

    // Status and Priority
    public int StatusId { get; set; }
    public string StatusName { get; set; } = null!;
    public string? StatusColor { get; set; }
    public string? Priority { get; set; }

    /// <summary>
    /// Nguồn gốc WorkOrder: "Scheduled" (từ appointment) hoặc "WalkIn" (khách walk-in)
    /// </summary>
    public string? SourceType { get; set; }

    // Dates
    public DateTime? StartDate { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime CreatedDate { get; set; }

    // Assigned staff
    public int? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }

    public int? AdvisorId { get; set; }
    public string? AdvisorName { get; set; }

    public int? SupervisorId { get; set; }
    public string? SupervisorName { get; set; }

    // Financial
    public decimal? EstimatedAmount { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? FinalAmount { get; set; }

    // Appointment-related view-only fields (copied from linked Appointment for FE convenience)
    public decimal? AppointmentEstimatedCost { get; set; }
    public decimal? AppointmentFinalCost { get; set; }
    public decimal? AppointmentOutstandingAmount { get; set; }
    public bool HasOutstandingAppointmentPayment { get; set; }

    // Progress tracking
    public decimal? ProgressPercentage { get; set; }
    public int? ChecklistCompleted { get; set; }
    public int? ChecklistTotal { get; set; }

    // Approval and Quality
    public bool? RequiresApproval { get; set; }
    public bool? ApprovalRequired { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovalNotes { get; set; }

    public bool? QualityCheckRequired { get; set; }
    public int? QualityCheckedBy { get; set; }
    public string? QualityCheckedByName { get; set; }
    public DateTime? QualityCheckDate { get; set; }
    public int? QualityRating { get; set; }

    // Notes
    public string? CustomerNotes { get; set; }
    public string? InternalNotes { get; set; }
    public string? TechnicianNotes { get; set; }

    // Services and Parts
    public List<WorkOrderServiceItemDto> Services { get; set; } = new();
    public List<WorkOrderPartItemDto> Parts { get; set; } = new();
}

/// <summary>
/// Service item in work order
/// </summary>
public class WorkOrderServiceItemDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string? ServiceDescription { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Part item in work order
/// </summary>
public class WorkOrderPartItemDto
{
    public int PartId { get; set; }
    public string PartName { get; set; } = null!;
    public string? PartNumber { get; set; }
    public decimal UnitPrice { get; set; }
    public int QuantityUsed { get; set; }
    public decimal TotalCost { get; set; }
    public string? Status { get; set; }
    public string? Notes { get; set; }
}
