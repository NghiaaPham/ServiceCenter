using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class WarrantyType
{
    [Key]
    [Column("WarrantyTypeID")]
    public int WarrantyTypeId { get; set; }

    [StringLength(50)]
    public string TypeName { get; set; } = null!;

    [StringLength(200)]
    public string? Description { get; set; }

    public int? DefaultPeriod { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("WarrantyType")]
    public virtual ICollection<Warranty> Warranties { get; set; } = new List<Warranty>();
}
