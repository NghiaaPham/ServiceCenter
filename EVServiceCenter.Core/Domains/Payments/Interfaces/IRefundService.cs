using EVServiceCenter.Core.Domains.Payments.Entities;

namespace EVServiceCenter.Core.Domains.Payments.Interfaces
{
    /// <summary>
    /// Service xử lý refund workflow
    /// ✅ FIX GAP #12: Refund logic implementation
    /// </summary>
    public interface IRefundService
    {
        /// <summary>
        /// Process refund từ Pending → Processing → Completed/Failed
        /// </summary>
        /// <param name="refundId">ID của refund request</param>
        /// <param name="processedBy">User ID người xử lý</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful</returns>
        Task<bool> ProcessRefundAsync(
            int refundId,
            int processedBy,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Process tất cả pending refunds (dùng cho background job)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Số lượng refunds đã process</returns>
        Task<int> ProcessPendingRefundsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get refund by ID with details
        /// </summary>
        Task<Refund?> GetRefundByIdAsync(int refundId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get refunds by appointment ID
        /// </summary>
        Task<List<Refund>> GetRefundsByAppointmentIdAsync(int appointmentId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancel refund request (nếu chưa process)
        /// </summary>
        Task<bool> CancelRefundAsync(int refundId, string reason, int cancelledBy, CancellationToken cancellationToken = default);
    }
}
