namespace EVServiceCenter.Core.Domains.Payments.DTOs.Responses;

/// <summary>
/// Payment response DTO with full details
/// </summary>
public class PaymentResponseDto
{
    public int PaymentId { get; set; }
    public string PaymentCode { get; set; } = null!;
    public int InvoiceId { get; set; }
    public string InvoiceCode { get; set; } = null!;
    public int? PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = null!;
    public decimal Amount { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? Fee { get; set; }
    public DateTime? PaymentDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Status { get; set; } = null!;
    public string? TransactionRef { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? ResponseCode { get; set; }
    public string? ResponseMessage { get; set; }
    public string? Notes { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundDate { get; set; }
    public string? RefundReason { get; set; }
    public int? CreatedBy { get; set; }
    public string? CreatedByName { get; set; }
    public DateTime CreatedDate { get; set; }
}
