using EVServiceCenter.Core.Domains.Payments.Interfaces;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.BackgroundServices
{
    /// <summary>
    /// 🔧 BACKGROUND SERVICE: Appointment Reconciliation
    ///
    /// Chạy định kỳ (mỗi 6 giờ) để:
    /// 1. Auto-cancel appointments Pending > 48h chưa thanh toán
    /// 2. Mark PaymentIntents Pending > 24h → Expired (✅ FIX GAP #11)
    /// 3. Auto-update PaymentStatus (nếu có intent Completed nhưng chưa update)
    /// 4. Release TimeSlots from expired/cancelled appointments
    ///
    /// CRITICAL: Đảm bảo data consistency và operational health
    /// </summary>
    public class AppointmentReconciliationService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentReconciliationService> _logger;
        private readonly TimeSpan _runInterval = TimeSpan.FromHours(6); // Ch?y m?i 6 gi?

        public AppointmentReconciliationService(
            IServiceProvider serviceProvider,
            ILogger<AppointmentReconciliationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public void Start()
        {
            _logger.LogInformation("? AppointmentReconciliationService started");

            // Run in background (fire and forget v?i proper error handling)
            _ = Task.Run(async () =>
            {
                // ??i 1 ph�t sau khi app start
                await Task.Delay(TimeSpan.FromMinutes(1));

                while (true)
                {
                    try
                    {
                        _logger.LogInformation("?? Starting reconciliation cycle at {Time}", DateTime.UtcNow);

                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<EVDbContext>();

                        await ReconcileAppointmentsAsync(context);
                        await ReconcilePaymentIntentsAsync(context);
                        await SyncPaymentStatusAsync(context);
                        await ProcessPendingRefundsAsync(scope.ServiceProvider);

                        _logger.LogInformation("? Reconciliation cycle completed at {Time}", DateTime.UtcNow);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "? Error in reconciliation cycle");
                    }

                    // ??i 6 gi? tr??c khi ch?y l?i
                    await Task.Delay(_runInterval);
                }
            });
        }

        /// <summary>
        /// 1?? Auto-cancel Pending appointments > 48h ch?a thanh to�n
        /// </summary>
        private async Task ReconcileAppointmentsAsync(EVDbContext context)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-48);
            const int batchSize = 200;
            int processedCount = 0;

            while (true)
            {
                var expiredAppointments = await context.Appointments
                    .Where(a =>
                        a.StatusId == (int)AppointmentStatusEnum.Pending &&
                        a.CreatedDate < cutoffTime &&
                        a.PaymentStatus != PaymentStatusEnum.Completed.ToString())
                    .OrderBy(a => a.AppointmentId)
                    .Take(batchSize)
                    .ToListAsync();

                if (!expiredAppointments.Any())
                {
                    if (processedCount == 0)
                    {
                        _logger.LogInformation("No expired appointments found");
                    }
                    break;
                }

                foreach (var appointment in expiredAppointments)
                {
                    appointment.StatusId = (int)AppointmentStatusEnum.Cancelled;
                    appointment.CancellationReason = "Auto-cancelled: No payment received within 48 hours";

                    _logger.LogInformation(
                        "Auto-cancelled appointment {AppointmentCode} (Created: {CreatedDate}, No payment for 48h)",
                        appointment.AppointmentCode,
                        appointment.CreatedDate);
                }

                await context.SaveChangesAsync();
                processedCount += expiredAppointments.Count;
                context.ChangeTracker.Clear();
            }

            if (processedCount > 0)
            {
                _logger.LogInformation(
                    "? Auto-cancelled {Count} expired appointments",
                    processedCount);
            }
        }

        /// <summary>
        /// 2️⃣ Mark PaymentIntents Pending > 24h → Expired
        /// ✅ FIX GAP #11: Payment expiration background job
        /// </summary>
        private async Task ReconcilePaymentIntentsAsync(EVDbContext context)
        {
            var now = DateTime.UtcNow;
            const int batchSize = 200;
            int processedCount = 0;

            while (true)
            {
                var expiredIntents = await context.PaymentIntents
                    .Include(pi => pi.Appointment)
                    .Where(pi =>
                        pi.Status == PaymentIntentStatusEnum.Pending.ToString() &&
                        pi.ExpiresAt < now)
                    .OrderBy(pi => pi.PaymentIntentId)
                    .Take(batchSize)
                    .ToListAsync();

                if (!expiredIntents.Any())
                {
                    if (processedCount == 0)
                    {
                        _logger.LogInformation("GAP #11 - No expired payment intents found");
                    }
                    break;
                }

                _logger.LogInformation(
                    "GAP #11 - Processing {Count} expired payment intents",
                    expiredIntents.Count);

                foreach (var intent in expiredIntents)
                {
                    intent.Status = PaymentIntentStatusEnum.Expired.ToString();
                    intent.UpdatedDate = now;

                    var hoursExpired = intent.ExpiresAt.HasValue
                        ? (now - intent.ExpiresAt.Value).TotalHours
                        : 0;

                    _logger.LogInformation(
                        "GAP #11 - Expired PaymentIntent: {IntentCode} (AppointmentId={AppointmentId}, Amount={Amount}, Expired {Hours:F1}h ago)",
                        intent.IntentCode,
                        intent.AppointmentId,
                        intent.Amount,
                        hoursExpired);

                    if (intent.Appointment != null &&
                        intent.Appointment.LatestPaymentIntentId == intent.PaymentIntentId &&
                        intent.Appointment.PaymentStatus == PaymentStatusEnum.Pending.ToString())
                    {
                        var hasCompletedIntent = await context.PaymentIntents
                            .AsNoTracking()
                            .AnyAsync(pi => pi.AppointmentId == intent.AppointmentId
                                         && pi.Status == PaymentIntentStatusEnum.Completed.ToString());

                        if (!hasCompletedIntent)
                        {
                            intent.Appointment.PaymentStatus = PaymentStatusEnum.Failed.ToString();
                            _logger.LogWarning(
                                "Updated Appointment {AppointmentCode} PaymentStatus: Pending -> Failed (no valid payment)",
                                intent.Appointment.AppointmentCode);
                        }
                    }
                }

                await context.SaveChangesAsync();
                processedCount += expiredIntents.Count;
                context.ChangeTracker.Clear();
            }

            if (processedCount > 0)
            {
                _logger.LogInformation(
                    "GAP #11 - Marked {Count} payment intents as Expired",
                    processedCount);
            }
        }

        /// <summary>
        /// 3?? Sync PaymentStatus (n?u c� intent Completed nh?ng appointment ch?a update)
        /// </summary>
        private async Task SyncPaymentStatusAsync(EVDbContext context)
        {
            const int batchSize = 200;
            int processedCount = 0;

            while (true)
            {
                var inconsistentAppointments = await context.Appointments
                    .Include(a => a.PaymentIntents)
                    .Where(a =>
                        a.PaymentStatus != PaymentStatusEnum.Completed.ToString() &&
                        a.PaymentIntents.Any(pi => pi.Status == PaymentIntentStatusEnum.Completed.ToString()))
                    .OrderBy(a => a.AppointmentId)
                    .Take(batchSize)
                    .ToListAsync();

                if (!inconsistentAppointments.Any())
                {
                    if (processedCount == 0)
                    {
                        _logger.LogInformation("No payment status inconsistencies found");
                    }
                    break;
                }

                foreach (var appointment in inconsistentAppointments)
                {
                    var completedIntents = appointment.PaymentIntents
                        .Where(pi => pi.Status == PaymentIntentStatusEnum.Completed.ToString())
                        .ToList();

                    var totalPaid = completedIntents.Sum(pi => pi.Amount);
                    var finalCost = appointment.FinalCost ?? appointment.EstimatedCost ?? 0m;

                    appointment.PaidAmount = totalPaid;

                    if (totalPaid >= finalCost)
                    {
                        appointment.PaymentStatus = PaymentStatusEnum.Completed.ToString();

                        _logger.LogInformation(
                            "Auto-synced PaymentStatus for {AppointmentCode}: {Paid} / {Total} -> Completed",
                            appointment.AppointmentCode,
                            totalPaid,
                            finalCost);
                    }
                    else
                    {
                        appointment.PaymentStatus = PaymentStatusEnum.Pending.ToString();

                        _logger.LogWarning(
                            "Partial payment for {AppointmentCode}: {Paid} / {Total} -> Pending",
                            appointment.AppointmentCode,
                            totalPaid,
                            finalCost);
                    }
                }

                await context.SaveChangesAsync();
                processedCount += inconsistentAppointments.Count;
                context.ChangeTracker.Clear();
            }

            if (processedCount > 0)
            {
                _logger.LogInformation(
                    "Synced payment status for {Count} appointments",
                    processedCount);
            }
        }

        /// <summary>
        /// 4️⃣ Process pending refunds
        /// ✅ FIX GAP #12: Auto-process refund requests in background
        /// </summary>
        private async Task ProcessPendingRefundsAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var refundService = serviceProvider.GetService<IRefundService>();

                if (refundService == null)
                {
                    _logger.LogWarning("⚠️ GAP #12 - IRefundService not registered, skipping refund processing");
                    return;
                }

                _logger.LogInformation("🔧 GAP #12 - Starting pending refunds processing");

                int processedCount = await refundService.ProcessPendingRefundsAsync();

                if (processedCount > 0)
                {
                    _logger.LogInformation(
                        "✅ GAP #12 - Processed {Count} refunds successfully",
                        processedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ GAP #12 - Error processing pending refunds");
            }
        }
    }
}











