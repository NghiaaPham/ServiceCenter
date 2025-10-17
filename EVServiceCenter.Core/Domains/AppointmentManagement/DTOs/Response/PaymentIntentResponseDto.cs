using System;
using System.Collections.Generic;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Response
{
  public class PaymentIntentResponseDto
  {
    public int PaymentIntentId { get; set; }

    public string IntentCode { get; set; } = null!;

    public decimal Amount { get; set; }

    public decimal? CapturedAmount { get; set; }

    public decimal? RefundedAmount { get; set; }

    public string Currency { get; set; } = "VND";

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public DateTime? ConfirmedDate { get; set; }

    public DateTime? CancelledDate { get; set; }

    public DateTime? FailedDate { get; set; }

    public DateTime? ExpiredDate { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Notes { get; set; }

    public List<PaymentTransactionResponseDto> Transactions { get; set; } = new();
  }
}
