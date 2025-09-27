using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Identity.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EVServiceCenter.Core.Entities;

public partial class Invoice
{
    [Key]
    [Column("InvoiceID")]
    public int InvoiceId { get; set; }

    [StringLength(20)]
    public string InvoiceCode { get; set; } = null!;

    [Column("WorkOrderID")]
    public int WorkOrderId { get; set; }

    [Column("CustomerID")]
    public int CustomerId { get; set; }

    public DateTime? InvoiceDate { get; set; }

    public DateOnly? DueDate { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? ServiceSubTotal { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? ServiceDiscount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? ServiceTax { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? PartsSubTotal { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? PartsDiscount { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? PartsTax { get; set; }

    [Column(TypeName = "decimal(17, 2)")]
    public decimal? ServiceTotal { get; set; }

    [Column(TypeName = "decimal(17, 2)")]
    public decimal? PartsTotal { get; set; }

    [Column(TypeName = "decimal(16, 2)")]
    public decimal? SubTotal { get; set; }

    [Column(TypeName = "decimal(16, 2)")]
    public decimal? TotalDiscount { get; set; }

    [Column(TypeName = "decimal(16, 2)")]
    public decimal? TotalTax { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? GrandTotal { get; set; }

    [Column(TypeName = "decimal(15, 2)")]
    public decimal? PaidAmount { get; set; }

    [Column(TypeName = "decimal(19, 2)")]
    public decimal? OutstandingAmount { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    [StringLength(100)]
    public string? PaymentTerms { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool? SentToCustomer { get; set; }

    public DateTime? SentDate { get; set; }

    [StringLength(20)]
    public string? SentMethod { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    [InverseProperty("RelatedInvoice")]
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    [ForeignKey("CreatedBy")]
    [InverseProperty("InvoiceCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Invoices")]
    public virtual Customer Customer { get; set; } = null!;

    [InverseProperty("Invoice")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("UpdatedBy")]
    [InverseProperty("InvoiceUpdatedByNavigations")]
    public virtual User? UpdatedByNavigation { get; set; }

    [ForeignKey("WorkOrderId")]
    [InverseProperty("Invoices")]
    public virtual WorkOrder WorkOrder { get; set; } = null!;
}
