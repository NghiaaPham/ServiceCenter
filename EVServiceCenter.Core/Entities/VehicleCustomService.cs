using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class VehicleCustomService
{
    [Key]
    [Column("CustomServiceID")]
    public int CustomServiceId { get; set; }

    [Column("VehicleID")]
    public int VehicleId { get; set; }

    [StringLength(200)]
    public string ServiceName { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? CustomPrice { get; set; }

    public int? EstimatedTime { get; set; }

    public bool? IsRecurring { get; set; }

    public int? RecurrenceInterval { get; set; }

    public DateOnly? NextDueDate { get; set; }

    public DateOnly? LastPerformedDate { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("VehicleCustomServices")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("VehicleId")]
    [InverseProperty("VehicleCustomServices")]
    public virtual CustomerVehicle Vehicle { get; set; } = null!;
}
