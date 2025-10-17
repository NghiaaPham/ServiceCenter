using EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Core.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.AppointmentManagement.Services
{
    public class AppointmentReconciliationService : IAppointmentReconciliationService
    {
        private readonly EVDbContext _context;
        private readonly ILogger<AppointmentReconciliationService> _logger;

        public AppointmentReconciliationService(
            EVDbContext context,
            ILogger<AppointmentReconciliationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Auto-cancel appointments Pending > 48h chưa thanh toán
        /// </summary>
        public async Task<int> AutoCancelExpiredAppointmentsAsync(CancellationToken cancellationToken = default)
        {
            var cutoffDate = DateTime.UtcNow.AddHours(-48);

            var expiredAppointments = await _context.Appointments
                .Where(a => a.StatusId == (int)AppointmentStatusEnum.Pending
                         && a.PaymentStatus == PaymentStatusEnum.Pending.ToString()
                         && a.CreatedDate < cutoffDate)
                .ToListAsync(cancellationToken);

            if (!expiredAppointments.Any())
            {
                _logger.LogInformation("✅ No expired appointments to auto-cancel");
                return 0;
            }

            foreach (var appointment in expiredAppointments)
            {
                appointment.StatusId = (int)AppointmentStatusEnum.Cancelled;
                appointment.CancelledDate = DateTime.UtcNow;
                appointment.CancellationReason = "AUTO_CANCEL: Quá 48h chưa thanh toán";
                appointment.UpdatedDate = DateTime.UtcNow;

                _logger.LogWarning(
                    "⚠️ Auto-cancelled appointment {AppointmentId} (Code: {Code}, Created: {CreatedDate}, > 48h unpaid)",
                    appointment.AppointmentId, appointment.AppointmentCode, appointment.CreatedDate);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "✅ Auto-cancelled {Count} expired appointments",
                expiredAppointments.Count);

            return expiredAppointments.Count;
        }

        /// <summary>
        /// Auto-expire PaymentIntent Pending > ExpiresAt
        /// </summary>
        public async Task<int> AutoExpirePaymentIntentsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var expiredIntents = await _context.PaymentIntents
                .Where(pi => pi.Status == PaymentIntentStatusEnum.Pending.ToString()
                          && pi.ExpiresAt < now)
                .ToListAsync(cancellationToken);

            if (!expiredIntents.Any())
            {
                _logger.LogInformation("✅ No expired payment intents");
                return 0;
            }

            foreach (var intent in expiredIntents)
            {
                intent.Status = PaymentIntentStatusEnum.Expired.ToString();
                intent.ExpiredDate = DateTime.UtcNow;
                intent.UpdatedDate = DateTime.UtcNow;

                _logger.LogWarning(
                    "⚠️ Auto-expired PaymentIntent {IntentCode} (ExpiresAt: {ExpiresAt})",
                    intent.IntentCode, intent.ExpiresAt);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "✅ Auto-expired {Count} payment intents",
                expiredIntents.Count);

            return expiredIntents.Count;
        }

        /// <summary>
        /// Sync PaymentStatus với PaymentIntent status
        /// Fix data inconsistency
        /// </summary>
        public async Task<int> SyncPaymentStatusAsync(CancellationToken cancellationToken = default)
        {
            // Tìm appointments có PaymentIntent.Completed nhưng PaymentStatus != Paid
            var inconsistentAppointments = await _context.Appointments
                .Where(a => a.PaymentIntents.Any(pi => pi.Status == PaymentIntentStatusEnum.Completed.ToString())
                         && a.PaymentStatus != PaymentStatusEnum.Completed.ToString())
                .Include(a => a.PaymentIntents)
                .ToListAsync(cancellationToken);

            if (!inconsistentAppointments.Any())
            {
                _logger.LogInformation("✅ All payment statuses are synced");
                return 0;
            }

            int fixedCount = 0;

            foreach (var appointment in inconsistentAppointments)
            {
                var completedIntents = appointment.PaymentIntents
                    .Where(pi => pi.Status == PaymentIntentStatusEnum.Completed.ToString())
                    .ToList();

                decimal totalCaptured = completedIntents.Sum(pi => pi.CapturedAmount ?? 0);
                decimal finalCost = appointment.FinalCost ?? appointment.EstimatedCost ?? 0;

                string oldStatus = appointment.PaymentStatus;

                // Update payment status dựa trên tổng tiền đã capture
                if (totalCaptured >= finalCost)
                {
                    appointment.PaymentStatus = PaymentStatusEnum.Completed.ToString();
                }
                else if (totalCaptured > 0)
                {
                    appointment.PaymentStatus = PaymentStatusEnum.Pending.ToString(); // Partial payment
                }

                appointment.PaidAmount = totalCaptured;
                appointment.UpdatedDate = DateTime.UtcNow;

                _logger.LogWarning(
                    "🔄 Synced payment status for appointment {AppointmentId}: {OldStatus} → {NewStatus} " +
                    "(Captured: {Captured}đ, FinalCost: {FinalCost}đ)",
                    appointment.AppointmentId, oldStatus, appointment.PaymentStatus,
                    totalCaptured, finalCost);

                fixedCount++;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "✅ Synced payment status for {Count} appointments",
                fixedCount);

            return fixedCount;
        }

        /// <summary>
        /// Tạo báo cáo đối soát hàng ngày
        /// ✅ OPTIMIZED: Batch queries (15+ queries → 4 queries)
        /// </summary>
        public async Task<ReconciliationReportDto> GenerateDailyReconciliationReportAsync(
            DateTime date,
            CancellationToken cancellationToken = default)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1);

            _logger.LogInformation(
                "📊 Generating reconciliation report for {Date}",
                date.ToString("yyyy-MM-dd"));

            var report = new ReconciliationReportDto
            {
                ReportDate = date,
                GeneratedAt = DateTime.UtcNow
            };

            // ✅ OPTIMIZED: Single query for all appointment stats
            var appointmentStats = await _context.Appointments
                .Where(a => a.CreatedDate >= startOfDay && a.CreatedDate < endOfDay)
                .GroupBy(a => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Pending = g.Count(a => a.StatusId == (int)AppointmentStatusEnum.Pending),
                    Confirmed = g.Count(a => a.StatusId == (int)AppointmentStatusEnum.Confirmed),
                    InProgress = g.Count(a => a.StatusId == (int)AppointmentStatusEnum.InProgress),
                    Completed = g.Count(a => a.StatusId == (int)AppointmentStatusEnum.Completed),
                    Cancelled = g.Count(a => a.StatusId == (int)AppointmentStatusEnum.Cancelled),
                    AutoCancelled = g.Count(a => a.CancellationReason != null &&
                                                 a.CancellationReason.StartsWith("AUTO_CANCEL"))
                })
                .FirstOrDefaultAsync(cancellationToken);

            report.TotalAppointments = appointmentStats?.Total ?? 0;
            report.PendingAppointments = appointmentStats?.Pending ?? 0;
            report.ConfirmedAppointments = appointmentStats?.Confirmed ?? 0;
            report.InProgressAppointments = appointmentStats?.InProgress ?? 0;
            report.CompletedAppointments = appointmentStats?.Completed ?? 0;
            report.CancelledAppointments = appointmentStats?.Cancelled ?? 0;
            report.AutoCancelledCount = appointmentStats?.AutoCancelled ?? 0;

            // ✅ OPTIMIZED: Single query for all payment stats
            var paymentStats = await _context.PaymentIntents
                .Where(pi => pi.CreatedDate >= startOfDay && pi.CreatedDate < endOfDay)
                .GroupBy(pi => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Completed = g.Count(pi => pi.Status == PaymentIntentStatusEnum.Completed.ToString()),
                    Pending = g.Count(pi => pi.Status == PaymentIntentStatusEnum.Pending.ToString()),
                    Expired = g.Count(pi => pi.Status == PaymentIntentStatusEnum.Expired.ToString()),
                    PendingAmount = g.Where(pi => pi.Status == PaymentIntentStatusEnum.Pending.ToString())
                                     .Sum(pi => pi.Amount)
                })
                .FirstOrDefaultAsync(cancellationToken);

            report.TotalPaymentIntents = paymentStats?.Total ?? 0;
            report.CompletedPaymentIntents = paymentStats?.Completed ?? 0;
            report.PendingPaymentIntents = paymentStats?.Pending ?? 0;
            report.ExpiredPaymentIntents = paymentStats?.Expired ?? 0;
            report.TotalPendingAmount = paymentStats?.PendingAmount ?? 0;

            // Revenue query (separate because different date filter)
            report.TotalRevenue = await _context.PaymentIntents
                .Where(pi => pi.ConfirmedDate >= startOfDay && pi.ConfirmedDate < endOfDay
                          && pi.Status == PaymentIntentStatusEnum.Completed.ToString())
                .SumAsync(pi => pi.CapturedAmount ?? 0, cancellationToken);

            // ✅ OPTIMIZED: Single query for all refund stats
            var refundStats = await _context.Refunds
                .Where(r => r.CreatedDate >= startOfDay && r.CreatedDate < endOfDay)
                .GroupBy(r => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Processed = g.Count(r => r.Status == RefundConstants.Status.Completed),
                    Pending = g.Count(r => r.Status == RefundConstants.Status.Pending
                                         || r.Status == RefundConstants.Status.Processing),
                    TotalAmount = g.Where(r => r.Status == RefundConstants.Status.Completed)
                                   .Sum(r => r.RefundAmount)
                })
                .FirstOrDefaultAsync(cancellationToken);

            report.TotalRefunds = refundStats?.Total ?? 0;
            report.ProcessedRefunds = refundStats?.Processed ?? 0;
            report.PendingRefunds = refundStats?.Pending ?? 0;
            report.TotalRefundAmount = refundStats?.TotalAmount ?? 0;

            // ✅ OPTIMIZED: Single query for issues
            var issueStats = await _context.Appointments
                .GroupBy(a => 1)
                .Select(g => new
                {
                    PaymentMismatch = g.Count(a => a.PaymentIntents.Any(pi => pi.Status == PaymentIntentStatusEnum.Completed.ToString())
                                                 && a.PaymentStatus != PaymentStatusEnum.Completed.ToString()),
                    UnpaidBalance = g.Count(a => a.StatusId == (int)AppointmentStatusEnum.CompletedWithUnpaidBalance)
                })
                .FirstOrDefaultAsync(cancellationToken);

            report.PaymentStatusMismatchCount = issueStats?.PaymentMismatch ?? 0;
            report.UnpaidBalanceCount = issueStats?.UnpaidBalance ?? 0;

            // ===== WARNINGS =====
            if (report.AutoCancelledCount > 10)
            {
                report.Warnings.Add($"⚠️ High auto-cancellation rate: {report.AutoCancelledCount} appointments");
            }

            if (report.PaymentStatusMismatchCount > 0)
            {
                report.Warnings.Add($"⚠️ Payment status mismatch: {report.PaymentStatusMismatchCount} appointments");
            }

            if (report.PendingRefunds > 5)
            {
                report.Warnings.Add($"⚠️ High pending refunds: {report.PendingRefunds} refunds");
            }

            if (report.UnpaidBalanceCount > 0)
            {
                report.Warnings.Add($"⚠️ Unpaid balance: {report.UnpaidBalanceCount} appointments");
            }

            _logger.LogInformation(
                "✅ Reconciliation report generated: {TotalAppointments} appointments, " +
                "{TotalRevenue}đ revenue, {TotalRefunds} refunds, {Warnings} warnings",
                report.TotalAppointments, report.TotalRevenue,
                report.TotalRefunds, report.Warnings.Count);

            return report;
        }
    }
}
