using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class PaymentMethod
{
    [Key]
    [Column("MethodID")]
    public int MethodId { get; set; }

    [StringLength(20)]
    public string MethodCode { get; set; } = null!;

    [StringLength(50)]
    public string MethodName { get; set; } = null!;

    [StringLength(20)]
    public string PaymentType { get; set; } = null!;

    [StringLength(100)]
    public string? GatewayProvider { get; set; }

    public string? ProviderConfig { get; set; }

    [Column(TypeName = "decimal(5, 4)")]
    public decimal? ProcessingFee { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? FixedFee { get; set; }

    public bool? IsOnline { get; set; }

    public bool? RequiresApproval { get; set; }

    public bool? IsActive { get; set; }

    public int? DisplayOrder { get; set; }

    [StringLength(500)]
    public string? IconUrl { get; set; }

    [InverseProperty("PaymentMethod")]
    public virtual ICollection<CustomerPackageSubscription> CustomerPackageSubscriptions { get; set; } = new List<CustomerPackageSubscription>();

    [InverseProperty("Method")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
