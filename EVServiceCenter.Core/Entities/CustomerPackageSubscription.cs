using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class CustomerPackageSubscription
{
    [Key]
    [Column("SubscriptionID")]
    public int SubscriptionId { get; set; }

    [StringLength(20)]
    public string SubscriptionCode { get; set; } = null!;

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Column("PackageID")]
    public int PackageId { get; set; }

    [Column("VehicleID")]
    public int? VehicleId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? ExpirationDate { get; set; }

    public DateOnly? RenewalDate { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public bool? AutoRenew { get; set; }

    [Column("PaymentMethodID")]
    public int? PaymentMethodId { get; set; }

    public int? RemainingServices { get; set; }

    public int? UsedServices { get; set; }

    public DateOnly? LastServiceDate { get; set; }

    public DateOnly? NextPaymentDate { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? PaymentAmount { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? DiscountPercent { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateOnly? CancelledDate { get; set; }

    [StringLength(500)]
    public string? CancellationReason { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("CustomerPackageSubscriptions")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerPackageSubscriptions")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("PackageId")]
    [InverseProperty("CustomerPackageSubscriptions")]
    public virtual MaintenancePackage Package { get; set; } = null!;

    [ForeignKey("PaymentMethodId")]
    [InverseProperty("CustomerPackageSubscriptions")]
    public virtual PaymentMethod? PaymentMethod { get; set; }

    [ForeignKey("VehicleId")]
    [InverseProperty("CustomerPackageSubscriptions")]
    public virtual CustomerVehicle? Vehicle { get; set; }
}
