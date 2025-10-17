using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using EVServiceCenter.Core.Domains.Payments.Entities;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.Payments.Interfaces.Services;
using EVServiceCenter.Core.Enums;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.Payments.Services
{
  public class PaymentIntentService : IPaymentIntentService
  {
    private readonly IPaymentIntentRepository _repository;
    private readonly ILogger<PaymentIntentService> _logger;

    public PaymentIntentService(
        IPaymentIntentRepository repository,
        ILogger<PaymentIntentService> logger)
    {
      _repository = repository;
      _logger = logger;
    }

    public PaymentIntent BuildPendingIntent(
        int customerId,
        decimal amount,
        int createdBy,
        string? currency = null,
        DateTime? expiresAt = null,
        string? paymentMethod = null,
        string? idempotencyKey = null)
    {
      var normalizedAmount = Math.Round(amount, 2, MidpointRounding.AwayFromZero);
      var targetCurrency = string.IsNullOrWhiteSpace(currency) ? "VND" : currency!;
      var dueDate = expiresAt ?? DateTime.UtcNow.AddHours(24);

      return new PaymentIntent
      {
        CustomerId = customerId,
        Amount = normalizedAmount,
        CapturedAmount = 0,
        RefundedAmount = 0,
        Currency = targetCurrency,
        Status = PaymentIntentStatusEnum.Pending.ToString(),
        CreatedDate = DateTime.UtcNow,
        CreatedBy = createdBy,
        ExpiresAt = dueDate,
        DueDate = dueDate,
        PaymentMethod = paymentMethod,
        IdempotencyKey = idempotencyKey,
        IntentCode = GenerateIntentCode(),
        Notes = null,
        MetadataJson = null
      };
    }

    public async Task<PaymentIntent> AppendNewIntentAsync(
        PaymentIntent intent,
        CancellationToken cancellationToken = default)
    {
      if (intent.PaymentIntentId != 0)
      {
        throw new InvalidOperationException("Intent đã tồn tại, không thể tạo mới");
      }

      await _repository.AddAsync(intent, cancellationToken);
      _logger.LogInformation(
          "Created payment intent {IntentCode} for appointment {AppointmentId}",
          intent.IntentCode,
          intent.AppointmentId);

      return intent;
    }

    public Task<PaymentIntent?> GetLatestByAppointmentAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
      return _repository.GetLatestByAppointmentAsync(appointmentId, cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentIntent>> GetByAppointmentAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
      var intents = await _repository.GetByAppointmentAsync(appointmentId, cancellationToken);
      return intents;
    }

    public async Task<PaymentIntent> MarkCompletedAsync(
        int paymentIntentId,
        decimal capturedAmount,
        int updatedBy,
        CancellationToken cancellationToken = default)
    {
      var intent = await LoadIntent(paymentIntentId, cancellationToken);

      intent.Status = PaymentIntentStatusEnum.Completed.ToString();
      intent.CapturedAmount = Math.Round(capturedAmount, 2, MidpointRounding.AwayFromZero);
      intent.UpdatedBy = updatedBy;
      intent.UpdatedDate = DateTime.UtcNow;
      intent.ConfirmedDate = DateTime.UtcNow;

      await _repository.UpdateAsync(intent, cancellationToken);

      _logger.LogInformation(
          "Marked payment intent {IntentCode} as completed", intent.IntentCode);

      return intent;
    }

    public async Task<PaymentIntent> MarkCancelledAsync(
        int paymentIntentId,
        string reason,
        int updatedBy,
        CancellationToken cancellationToken = default)
    {
      var intent = await LoadIntent(paymentIntentId, cancellationToken);

      intent.Status = PaymentIntentStatusEnum.Cancelled.ToString();
      intent.CancelledDate = DateTime.UtcNow;
      intent.UpdatedBy = updatedBy;
      intent.UpdatedDate = DateTime.UtcNow;
      intent.Notes = reason;

      await _repository.UpdateAsync(intent, cancellationToken);

      _logger.LogInformation(
          "Cancelled payment intent {IntentCode} with reason {Reason}",
          intent.IntentCode,
          reason);

      return intent;
    }

    public async Task<PaymentIntent> MarkExpiredAsync(
        int paymentIntentId,
        int updatedBy,
        CancellationToken cancellationToken = default)
    {
      var intent = await LoadIntent(paymentIntentId, cancellationToken);

      intent.Status = PaymentIntentStatusEnum.Expired.ToString();
      intent.ExpiredDate = DateTime.UtcNow;
      intent.UpdatedBy = updatedBy;
      intent.UpdatedDate = DateTime.UtcNow;

      await _repository.UpdateAsync(intent, cancellationToken);

      _logger.LogInformation(
          "Expired payment intent {IntentCode}", intent.IntentCode);

      return intent;
    }

    public async Task<PaymentIntent> MarkFailedAsync(
        int paymentIntentId,
        string reason,
        int updatedBy,
        CancellationToken cancellationToken = default)
    {
      var intent = await LoadIntent(paymentIntentId, cancellationToken);

      intent.Status = PaymentIntentStatusEnum.Failed.ToString();
      intent.FailedDate = DateTime.UtcNow;
      intent.UpdatedBy = updatedBy;
      intent.UpdatedDate = DateTime.UtcNow;
      intent.Notes = reason;

      await _repository.UpdateAsync(intent, cancellationToken);

      _logger.LogWarning(
          "Marked payment intent {IntentCode} as failed: {Reason}",
          intent.IntentCode,
          reason);

      return intent;
    }

    private async Task<PaymentIntent> LoadIntent(
        int paymentIntentId,
        CancellationToken cancellationToken)
    {
      var intent = await _repository.GetByIdAsync(paymentIntentId, cancellationToken);
      if (intent == null)
      {
        throw new InvalidOperationException($"PaymentIntent {paymentIntentId} không tồn tại");
      }

      return intent;
    }

    private static string GenerateIntentCode()
    {
      Span<byte> randomBytes = stackalloc byte[6];
      RandomNumberGenerator.Fill(randomBytes);
      var numericPart = BitConverter.ToUInt32(randomBytes);
      var truncated = numericPart % 1_000_000u;
      return $"PI-{DateTime.UtcNow:yyyyMMddHHmmss}-{truncated:D6}";
    }
  }
}
