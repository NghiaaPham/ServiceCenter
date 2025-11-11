namespace EVServiceCenter.Core.Domains.Invoices.DTOs.Responses;

/// <summary>
/// Response DTO for invoice details
/// </summary>
public class InvoiceResponseDto
{
    public int InvoiceId { get; set; }
    public string InvoiceCode { get; set; } = null!;
    public int? WorkOrderId { get; set; }
    public string? WorkOrderCode { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }

    // Dates
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsOverdue { get; set; }
    public int DaysOverdue { get; set; }

    // Service Charges
    public decimal ServiceSubTotal { get; set; }
    public decimal ServiceDiscount { get; set; }
    public decimal ServiceTax { get; set; }
    public decimal ServiceTotal { get; set; }

    // Parts Charges
    public decimal PartsSubTotal { get; set; }
    public decimal PartsDiscount { get; set; }
    public decimal PartsTax { get; set; }
    public decimal PartsTotal { get; set; }

    // Totals
    public decimal SubTotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalTax { get; set; }
    public decimal GrandTotal { get; set; }

    // Payment
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal PaymentProgress { get; set; } // Percentage

    // Status
    public string Status { get; set; } = null!;
    public string? PaymentTerms { get; set; }
    public string? Notes { get; set; }

    // Sending
    public bool SentToCustomer { get; set; }
    public DateTime? SentDate { get; set; }
    public string? SentMethod { get; set; }

    // Line Items
    public List<InvoiceServiceLineDto> ServiceLines { get; set; } = new();
    public List<InvoicePartLineDto> PartLines { get; set; } = new();

    // Payments
    public List<InvoicePaymentDto> Payments { get; set; } = new();

    // Audit
    public DateTime CreatedDate { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
}

/// <summary>
/// Service line item in invoice
/// </summary>
public class InvoiceServiceLineDto
{
    public int ServiceId { get; set; }
    public string ServiceCode { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string? Description { get; set; }
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPrice { get; set; }
}

/// <summary>
/// Part line item in invoice
/// </summary>
public class InvoicePartLineDto
{
    public int PartId { get; set; }
    public string PartCode { get; set; } = null!;
    public string PartName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public int? WarrantyPeriod { get; set; }
}

/// <summary>
/// Payment record on invoice
/// </summary>
public class InvoicePaymentDto
{
    public int PaymentId { get; set; }
    public string PaymentCode { get; set; } = null!;
    public string PaymentMethod { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string Status { get; set; } = null!;
    public string? TransactionRef { get; set; }
}
