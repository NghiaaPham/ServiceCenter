using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class CarBrand
{
    [Key]
    [Column("BrandID")]
    public int BrandId { get; set; }

    [StringLength(50)]
    public string BrandName { get; set; } = null!;

    [StringLength(50)]
    public string? Country { get; set; }

    [StringLength(500)]
    public string? LogoUrl { get; set; }

    [StringLength(200)]
    public string? Website { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("Brand")]
    public virtual ICollection<CarModel> CarModels { get; set; } = new List<CarModel>();

    [InverseProperty("Brand")]
    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
