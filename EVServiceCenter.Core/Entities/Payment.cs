using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class Payment
{
    [Key]
    [Column("PaymentID")]
    public int PaymentId { get; set; }

    [StringLength(20)]
    public string PaymentCode { get; set; } = null!;

    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [Column("MethodID")]
    public int MethodId { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? ProcessingFee { get; set; }

    [Column(TypeName = "decimal(16, 2)")]
    public decimal? NetAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    [StringLength(100)]
    public string? TransactionRef { get; set; }

    [StringLength(100)]
    public string? BankRef { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [StringLength(500)]
    public string? FailureReason { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? RefundAmount { get; set; }

    public DateTime? RefundDate { get; set; }

    [StringLength(500)]
    public string? RefundReason { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public int? ProcessedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    [ForeignKey("InvoiceId")]
    [InverseProperty("Payments")]
    public virtual Invoice Invoice { get; set; } = null!;

    [ForeignKey("MethodId")]
    [InverseProperty("Payments")]
    public virtual PaymentMethod Method { get; set; } = null!;

    [InverseProperty("Payment")]
    public virtual ICollection<OnlinePayment> OnlinePayments { get; set; } = new List<OnlinePayment>();

    [ForeignKey("ProcessedBy")]
    [InverseProperty("Payments")]
    public virtual User? ProcessedByNavigation { get; set; }
}
