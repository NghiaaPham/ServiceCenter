using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class Warranty
{
    [Key]
    [Column("WarrantyID")]
    public int WarrantyId { get; set; }

    [StringLength(20)]
    public string WarrantyCode { get; set; } = null!;

    [Column("WorkOrderID")]
    public int WorkOrderId { get; set; }

    [Column("ServiceID")]
    public int? ServiceId { get; set; }

    [Column("PartID")]
    public int? PartId { get; set; }

    [Column("WarrantyTypeID")]
    public int WarrantyTypeId { get; set; }

    public int? WarrantyPeriod { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int? Mileage { get; set; }

    public int? MileageLimit { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public string? Terms { get; set; }

    public DateOnly? ClaimedDate { get; set; }

    [Column("ClaimedWorkOrderID")]
    public int? ClaimedWorkOrderId { get; set; }

    public DateOnly? VoidDate { get; set; }

    [StringLength(500)]
    public string? VoidReason { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("ClaimedWorkOrderId")]
    [InverseProperty("WarrantyClaimedWorkOrders")]
    public virtual WorkOrder? ClaimedWorkOrder { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Warranties")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("PartId")]
    [InverseProperty("Warranties")]
    public virtual Part? Part { get; set; }

    [ForeignKey("ServiceId")]
    [InverseProperty("Warranties")]
    public virtual MaintenanceService? Service { get; set; }

    [ForeignKey("WarrantyTypeId")]
    [InverseProperty("Warranties")]
    public virtual WarrantyType WarrantyType { get; set; } = null!;

    [ForeignKey("WorkOrderId")]
    [InverseProperty("WarrantyWorkOrders")]
    public virtual WorkOrder WorkOrder { get; set; } = null!;
}
