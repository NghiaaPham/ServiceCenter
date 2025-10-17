namespace EVServiceCenter.Core.Domains.AppointmentManagement.Interfaces.Services
{
    /// <summary>
    /// Service để đối soát và tự động xử lý các trường hợp đặc biệt trong appointment lifecycle
    /// Run as background job (Hangfire/Quartz)
    /// </summary>
    public interface IAppointmentReconciliationService
    {
        /// <summary>
        /// Auto-cancel appointments Pending > 48h chưa thanh toán
        /// Run: Hàng giờ
        /// </summary>
        Task<int> AutoCancelExpiredAppointmentsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Auto-expire PaymentIntent Pending > ExpiresAt
        /// Run: Hàng giờ
        /// </summary>
        Task<int> AutoExpirePaymentIntentsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Auto-update PaymentStatus nếu có PaymentIntent.Completed nhưng PaymentStatus chưa Paid
        /// Data sync fix
        /// Run: Hàng ngày
        /// </summary>
        Task<int> SyncPaymentStatusAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Tạo báo cáo đối soát hàng ngày
        /// Tổng hợp: appointments, payments, refunds
        /// Run: Mỗi ngày lúc 00:00
        /// </summary>
        Task<ReconciliationReportDto> GenerateDailyReconciliationReportAsync(
            DateTime date,
            CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// DTO cho báo cáo đối soát
    /// </summary>
    public class ReconciliationReportDto
    {
        public DateTime ReportDate { get; set; }

        // Appointments
        public int TotalAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int ConfirmedAppointments { get; set; }
        public int InProgressAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int AutoCancelledCount { get; set; }

        // Payments
        public int TotalPaymentIntents { get; set; }
        public int CompletedPaymentIntents { get; set; }
        public int PendingPaymentIntents { get; set; }
        public int ExpiredPaymentIntents { get; set; }
        public int AutoExpiredCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalPendingAmount { get; set; }

        // Refunds
        public int TotalRefunds { get; set; }
        public int ProcessedRefunds { get; set; }
        public int PendingRefunds { get; set; }
        public decimal TotalRefundAmount { get; set; }

        // Issues
        public int PaymentStatusMismatchCount { get; set; }
        public int UnpaidBalanceCount { get; set; }

        public List<string> Warnings { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
    }
}
