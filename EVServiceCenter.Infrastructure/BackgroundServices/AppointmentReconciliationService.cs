using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.BackgroundServices
{
    /// <summary>
    /// ?? BACKGROUND SERVICE: Appointment Reconciliation
    /// 
    /// Ch?y ??nh k? (m?i 6 gi?) ??:
    /// 1. Auto-cancel appointments Pending > 48h ch?a thanh toán
    /// 2. Mark PaymentIntents Pending > 24h ? Expired
    /// 3. Auto-update PaymentStatus (n?u có intent Completed nh?ng ch?a update)
    /// 4. Generate daily metrics
    /// 
    /// CRITICAL: ??m b?o data consistency và operational health
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
                // ??i 1 phút sau khi app start
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
        /// 1?? Auto-cancel Pending appointments > 48h ch?a thanh toán
        /// </summary>
        private async Task ReconcileAppointmentsAsync(EVDbContext context)
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-48);

            var expiredAppointments = await context.Appointments
                .Where(a =>
                    a.StatusId == (int)AppointmentStatusEnum.Pending &&
                    a.CreatedDate < cutoffTime &&
                    a.PaymentStatus != PaymentStatusEnum.Completed.ToString())
                .ToListAsync();

            if (!expiredAppointments.Any())
            {
                _logger.LogInformation("No expired appointments found");
                return;
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

            _logger.LogInformation(
                "? Auto-cancelled {Count} expired appointments",
                expiredAppointments.Count);
        }

        /// <summary>
        /// 2?? Mark PaymentIntents Pending > 24h ? Expired
        /// </summary>
        private async Task ReconcilePaymentIntentsAsync(EVDbContext context)
        {
            var now = DateTime.UtcNow;

            var expiredIntents = await context.PaymentIntents
                .Where(pi =>
                    pi.Status == PaymentIntentStatusEnum.Pending.ToString() &&
                    pi.ExpiresAt < now)
                .ToListAsync();

            if (!expiredIntents.Any())
            {
                _logger.LogInformation("No expired payment intents found");
                return;
            }

            foreach (var intent in expiredIntents)
            {
                intent.Status = PaymentIntentStatusEnum.Expired.ToString();

                _logger.LogInformation(
                    "Marked PaymentIntent {IntentCode} as Expired (Expires: {ExpiresAt})",
                    intent.IntentCode,
                    intent.ExpiresAt);
            }

            await context.SaveChangesAsync();

            _logger.LogInformation(
                "? Marked {Count} payment intents as Expired",
                expiredIntents.Count);
        }

        /// <summary>
        /// 3?? Sync PaymentStatus (n?u có intent Completed nh?ng appointment ch?a update)
        /// </summary>
        private async Task SyncPaymentStatusAsync(EVDbContext context)
        {
            // Tìm appointments có PaymentIntent.Completed nh?ng PaymentStatus != Completed
            var inconsistentAppointments = await context.Appointments
                .Include(a => a.PaymentIntents)
                .Where(a =>
                    a.PaymentStatus != PaymentStatusEnum.Completed.ToString() &&
                    a.PaymentIntents.Any(pi => pi.Status == PaymentIntentStatusEnum.Completed.ToString()))
                .ToListAsync();

            if (!inconsistentAppointments.Any())
            {
                _logger.LogInformation("No payment status inconsistencies found");
                return;
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
                        "Auto-synced PaymentStatus for {AppointmentCode}: {Paid}? / {Total}? ? Completed",
                        appointment.AppointmentCode,
                        totalPaid,
                        finalCost);
                }
                else
                {
                    appointment.PaymentStatus = PaymentStatusEnum.Pending.ToString();

                    _logger.LogWarning(
                        "Partial payment for {AppointmentCode}: {Paid}? / {Total}? ? Pending",
                        appointment.AppointmentCode,
                        totalPaid,
                        finalCost);
                }
            }

            await context.SaveChangesAsync();

            _logger.LogInformation(
                "? Synced payment status for {Count} appointments",
                inconsistentAppointments.Count);
        }
    }
}

