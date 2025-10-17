using System;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
  public class CreatePaymentIntentRequestDto
  {
    public int AppointmentId { get; set; }

    public decimal? Amount { get; set; }

    public string Currency { get; set; } = "VND";

    public int? ExpiresInHours { get; set; }

    public string? PaymentMethod { get; set; }

    public string? Notes { get; set; }

    public string? IdempotencyKey { get; set; }
  }
}
