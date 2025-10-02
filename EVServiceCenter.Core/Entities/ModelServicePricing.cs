using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities; 

[Table("ModelServicePricing")]
public partial class ModelServicePricing
{
    [Key]
    [Column("PricingID")]
    public int PricingId { get; set; }

    [Column("ModelID")]
    public int ModelId { get; set; }

    [Column("ServiceID")]
    public int ServiceId { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? CustomPrice { get; set; }

    public int? CustomTime { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }

    // Navigation properties
    [ForeignKey("ModelId")]
    [InverseProperty("ModelServicePricings")]
    public virtual CarModel Model { get; set; } = null!;

    [ForeignKey("ServiceId")]
    [InverseProperty("ModelServicePricings")]
    public virtual MaintenanceService Service { get; set; } = null!;
}