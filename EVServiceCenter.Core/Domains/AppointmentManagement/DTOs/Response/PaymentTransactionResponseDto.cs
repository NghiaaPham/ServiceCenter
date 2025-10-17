using System;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
  public class PaymentTransactionResponseDto
  {
    public int PaymentTransactionId { get; set; }
    public int? PaymentIntentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Status { get; set; } = null!;
    public string? PaymentMethod { get; set; }
    public string? GatewayName { get; set; }
    public string? GatewayTransactionId { get; set; }
    public string? GatewayResponse { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? AuthorizedDate { get; set; }
    public DateTime? CapturedDate { get; set; }
    public DateTime? RefundedDate { get; set; }
    public DateTime? FailedDate { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
  }
}
