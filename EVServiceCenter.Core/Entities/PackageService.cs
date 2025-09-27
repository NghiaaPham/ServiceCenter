using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class PackageService
{
    [Key]
    [Column("PackageServiceID")]
    public int PackageServiceId { get; set; }

    [Column("PackageID")]
    public int PackageId { get; set; }

    [Column("ServiceID")]
    public int ServiceId { get; set; }

    public int? Quantity { get; set; }

    public bool? IncludedInPackage { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? AdditionalCost { get; set; }

    [ForeignKey("PackageId")]
    [InverseProperty("PackageServices")]
    public virtual MaintenancePackage Package { get; set; } = null!;

    [ForeignKey("ServiceId")]
    [InverseProperty("PackageServices")]
    public virtual MaintenanceService Service { get; set; } = null!;
}
