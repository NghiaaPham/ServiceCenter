using System;
using System.Threading;
using System.Threading.Tasks;
using EVServiceCenter.Core.Domains.Payments.Entities;

namespace EVServiceCenter.Core.Domains.Payments.Interfaces.Services
{
  public interface IPaymentIntentService
  {
    PaymentIntent BuildPendingIntent(
        int customerId,
        decimal amount,
        int createdBy,
        string? currency = null,
        DateTime? expiresAt = null,
        string? paymentMethod = null,
        string? idempotencyKey = null);

    Task<PaymentIntent> AppendNewIntentAsync(
        PaymentIntent intent,
        CancellationToken cancellationToken = default);

    Task<PaymentIntent?> GetLatestByAppointmentAsync(
        int appointmentId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PaymentIntent>> GetByAppointmentAsync(
        int appointmentId,
        CancellationToken cancellationToken = default);

    Task<PaymentIntent> MarkCompletedAsync(
        int paymentIntentId,
        decimal capturedAmount,
        int updatedBy,
        CancellationToken cancellationToken = default);

    Task<PaymentIntent> MarkCancelledAsync(
        int paymentIntentId,
        string reason,
        int updatedBy,
        CancellationToken cancellationToken = default);

    Task<PaymentIntent> MarkExpiredAsync(
        int paymentIntentId,
        int updatedBy,
        CancellationToken cancellationToken = default);

    Task<PaymentIntent> MarkFailedAsync(
        int paymentIntentId,
        string reason,
        int updatedBy,
        CancellationToken cancellationToken = default);
  }
}
