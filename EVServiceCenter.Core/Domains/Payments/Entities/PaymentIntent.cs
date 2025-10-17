using System;
using System.Collections.Generic;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Domains.Payments.Entities
{
  [Table("PaymentIntents")]
  public class PaymentIntent
  {
    [Key]
    [Column("PaymentIntentID")]
    public int PaymentIntentId { get; set; }

    [StringLength(30)]
    public string IntentCode { get; set; } = null!;

    [Column("AppointmentID")]
    public int AppointmentId { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? CapturedAmount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? RefundedAmount { get; set; }

    [StringLength(3)]
    public string Currency { get; set; } = "VND";

    [StringLength(20)]
    public string Status { get; set; } = PaymentIntentStatusEnum.Pending.ToString();

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? ConfirmedDate { get; set; }

    public DateTime? CancelledDate { get; set; }

    public DateTime? ExpiredDate { get; set; }

    public DateTime? FailedDate { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? DueDate { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    [StringLength(100)]
    public string? ProviderIntentId { get; set; }

    [StringLength(150)]
    public string? IdempotencyKey { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    public string? MetadataJson { get; set; }

    [ForeignKey("AppointmentId")]
    [InverseProperty("PaymentIntents")]
    public virtual Appointment Appointment { get; set; } = null!;

    [InverseProperty("PaymentIntent")]
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
  }
}
