using System;

namespace EVServiceCenter.Core.Domains.AppointmentManagement.DTOs.Request
{
  public class RecordPaymentResultRequestDto
  {
    public int AppointmentId { get; set; }

    public int PaymentIntentId { get; set; }

    public decimal Amount { get; set; }

    public string Status { get; set; } = null!; // Expected: Completed, Failed, Cancelled, Expired

    public string Currency { get; set; } = "VND";

    public string? PaymentMethod { get; set; }

    public string? GatewayName { get; set; }

    public string? GatewayTransactionId { get; set; }

    public string? GatewayResponse { get; set; }

    public string? Notes { get; set; }

    public DateTime? OccurredAt { get; set; }
  }
}
