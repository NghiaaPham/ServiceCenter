using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class WorkOrder
{
    [Key]
    [Column("WorkOrderID")]
    public int WorkOrderId { get; set; }

    [StringLength(20)]
    public string WorkOrderCode { get; set; } = null!;

    [Column("AppointmentID")]
    public int? AppointmentId { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Column("VehicleID")]
    public int VehicleId { get; set; }

    [Column("ServiceCenterID")]
    public int ServiceCenterId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EstimatedCompletionDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    [Column("StatusID")]
    public int StatusId { get; set; }

    [StringLength(20)]
    public string? Priority { get; set; }

    /// <summary>
    /// Nguồn gốc tạo WorkOrder: Scheduled (từ appointment) hoặc WalkIn (trực tiếp)
    /// </summary>
    [StringLength(20)]
    public string? SourceType { get; set; }

    [Column("TechnicianID")]
    public int? TechnicianId { get; set; }

    [Column("AdvisorID")]
    public int? AdvisorId { get; set; }

    [Column("SupervisorID")]
    public int? SupervisorId { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? EstimatedAmount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TotalAmount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? DiscountAmount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? TaxAmount { get; set; }

    [Column(TypeName = "decimal(17, 2)")]
    public decimal? FinalAmount { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? ProgressPercentage { get; set; }

    public int? ChecklistCompleted { get; set; }

    public int? ChecklistTotal { get; set; }

    public bool? RequiresApproval { get; set; }

    public bool? ApprovalRequired { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedDate { get; set; }

    [StringLength(500)]
    public string? ApprovalNotes { get; set; }

    public bool? QualityCheckRequired { get; set; }

    public int? QualityCheckedBy { get; set; }

    public DateTime? QualityCheckDate { get; set; }

    public int? QualityRating { get; set; }

    [StringLength(1000)]
    public string? CustomerNotes { get; set; }

    [StringLength(1000)]
    public string? InternalNotes { get; set; }

    [StringLength(1000)]
    public string? TechnicianNotes { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    [ForeignKey("AdvisorId")]
    [InverseProperty("WorkOrderAdvisors")]
    public virtual User? Advisor { get; set; }

    [ForeignKey("AppointmentId")]
    [InverseProperty("WorkOrders")]
    public virtual Appointment? Appointment { get; set; }

    [ForeignKey("ApprovedBy")]
    [InverseProperty("WorkOrderApprovedByNavigations")]
    public virtual User? ApprovedByNavigation { get; set; }

    [InverseProperty("RelatedWorkOrder")]
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    [InverseProperty("WorkOrder")]
    public virtual ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("WorkOrderCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("WorkOrders")]
    public virtual Customer Customer { get; set; } = null!;

    [InverseProperty("WorkOrder")]
    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    [InverseProperty("WorkOrder")]
    public virtual ICollection<MaintenanceHistory> MaintenanceHistories { get; set; } = new List<MaintenanceHistory>();

    [ForeignKey("QualityCheckedBy")]
    [InverseProperty("WorkOrderQualityCheckedByNavigations")]
    public virtual User? QualityCheckedByNavigation { get; set; }

    [ForeignKey("ServiceCenterId")]
    [InverseProperty("WorkOrders")]
    public virtual ServiceCenter ServiceCenter { get; set; } = null!;

    [InverseProperty("WorkOrder")]
    public virtual ICollection<ServiceRating> ServiceRatings { get; set; } = new List<ServiceRating>();

    [ForeignKey("StatusId")]
    [InverseProperty("WorkOrders")]
    public virtual WorkOrderStatus Status { get; set; } = null!;

    [ForeignKey("SupervisorId")]
    [InverseProperty("WorkOrderSupervisors")]
    public virtual User? Supervisor { get; set; }

    [ForeignKey("TechnicianId")]
    [InverseProperty("WorkOrderTechnicians")]
    public virtual User? Technician { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("WorkOrderUpdatedByNavigations")]
    public virtual User? UpdatedByNavigation { get; set; }

    [ForeignKey("VehicleId")]
    [InverseProperty("WorkOrders")]
    public virtual CustomerVehicle Vehicle { get; set; } = null!;

    [InverseProperty("WorkOrder")]
    public virtual ICollection<VehicleHealthMetric> VehicleHealthMetrics { get; set; } = new List<VehicleHealthMetric>();

    [InverseProperty("ClaimedWorkOrder")]
    public virtual ICollection<Warranty> WarrantyClaimedWorkOrders { get; set; } = new List<Warranty>();

    [InverseProperty("WorkOrder")]
    public virtual ICollection<Warranty> WarrantyWorkOrders { get; set; } = new List<Warranty>();

    [InverseProperty("WorkOrder")]
    public virtual ICollection<WorkOrderPart> WorkOrderParts { get; set; } = new List<WorkOrderPart>();

    [InverseProperty("WorkOrder")]
    public virtual ICollection<WorkOrderService> WorkOrderServices { get; set; } = new List<WorkOrderService>();

    [InverseProperty("WorkOrder")]
    public virtual ICollection<WorkOrderTimeline> WorkOrderTimelines { get; set; } = new List<WorkOrderTimeline>();
}
