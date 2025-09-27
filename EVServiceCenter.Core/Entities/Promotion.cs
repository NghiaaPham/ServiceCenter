using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class Promotion
{
    [Key]
    [Column("PromotionID")]
    public int PromotionId { get; set; }

    [StringLength(20)]
    public string PromotionCode { get; set; } = null!;

    [StringLength(100)]
    public string PromotionName { get; set; } = null!;

    [StringLength(20)]
    public string PromotionType { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? DiscountValue { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MinimumAmount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? MaximumDiscount { get; set; }

    public string? ApplicableServices { get; set; }

    public string? ApplicableCategories { get; set; }

    [StringLength(100)]
    public string? CustomerTypes { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public int? UsageLimit { get; set; }

    public int? UsageCount { get; set; }

    public bool? IsActive { get; set; }

    public string? Terms { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("Promotions")]
    public virtual User? CreatedByNavigation { get; set; }
}
