using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories
{
    /// <summary>
    /// Command Repository cho Package Subscription (CQRS Pattern)
    /// Chỉ chứa WRITE operations - Create, Update, Cancel
    /// </summary>
    public interface IPackageSubscriptionCommandRepository
    {
        /// <summary>
        /// Mua/Subscribe vào 1 package
        /// Flow:
        /// 1. Create CustomerPackageSubscription entity
        /// 2. Create PackageServiceUsage entries cho từng service trong package
        /// 3. Tính StartDate và ExpiryDate
        /// 4. Save changes
        /// 5. Return subscription details
        /// </summary>
        /// <param name="request">DTO chứa thông tin purchase</param>
        /// <param name="customerId">ID của customer mua package</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Full subscription details sau khi tạo</returns>
        Task<PackageSubscriptionResponseDto> PurchasePackageAsync(
            PurchasePackageRequestDto request,
            int customerId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Hủy subscription
        /// Set Status = Cancelled
        /// </summary>
        /// <param name="subscriptionId">ID của subscription cần hủy</param>
        /// <param name="cancellationReason">Lý do hủy</param>
        /// <param name="cancelledByUserId">ID của user hủy (customer hoặc staff)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu hủy thành công</returns>
        Task<bool> CancelSubscriptionAsync(
            int subscriptionId,
            string cancellationReason,
            int cancelledByUserId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Cập nhật usage sau khi customer dùng service trong appointment
        /// Tăng UsedQuantity, giảm RemainingQuantity
        /// Update LastUsedDate và LastUsedAppointmentId
        /// </summary>
        /// <param name="subscriptionId">ID của subscription</param>
        /// <param name="serviceId">ID của service vừa dùng</param>
        /// <param name="quantityUsed">Số lượng đã dùng (thường = 1)</param>
        /// <param name="appointmentId">ID của appointment</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu update thành công</returns>
        Task<bool> UpdateServiceUsageAsync(
            int subscriptionId,
            int serviceId,
            int quantityUsed,
            int appointmentId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tạm dừng subscription
        /// Set Status = Suspended
        /// VD: Customer di chuyển, tạm dừng xe,...
        /// </summary>
        /// <param name="subscriptionId">ID của subscription</param>
        /// <param name="reason">Lý do tạm dừng</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu thành công</returns>
        Task<bool> SuspendSubscriptionAsync(
            int subscriptionId,
            string reason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kích hoạt lại subscription đã bị suspend
        /// Set Status = Active
        /// </summary>
        /// <param name="subscriptionId">ID của subscription</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu thành công</returns>
        Task<bool> ReactivateSubscriptionAsync(
            int subscriptionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Auto-update status của subscriptions
        /// Chạy periodically (daily job) để check và update:
        /// - Expired subscriptions (qua ExpiryDate)
        /// - FullyUsed subscriptions (RemainingQuantity = 0 cho tất cả services)
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Số subscriptions đã được update</returns>
        Task<int> AutoUpdateExpiredSubscriptionsAsync(
            CancellationToken cancellationToken = default);
    }
}
