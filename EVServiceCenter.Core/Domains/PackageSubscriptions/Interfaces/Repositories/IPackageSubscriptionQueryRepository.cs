using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;
using EVServiceCenter.Core.Enums;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories
{
    /// <summary>
    /// Query Repository cho Package Subscription (CQRS Pattern)
    /// Chỉ chứa READ operations
    /// </summary>
    public interface IPackageSubscriptionQueryRepository
    {
        /// <summary>
        /// Lấy danh sách subscriptions của 1 customer
        /// </summary>
        /// <param name="customerId">ID của customer</param>
        /// <param name="statusFilter">Filter theo status (NULL = tất cả)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List subscription summaries</returns>
        Task<List<PackageSubscriptionSummaryDto>> GetCustomerSubscriptionsAsync(
            int customerId,
            SubscriptionStatusEnum? statusFilter = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy chi tiết đầy đủ 1 subscription
        /// Include all service usages
        /// </summary>
        /// <param name="subscriptionId">ID của subscription</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Full subscription details hoặc null</returns>
        Task<PackageSubscriptionResponseDto?> GetSubscriptionByIdAsync(
            int subscriptionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy subscriptions active cho 1 vehicle cụ thể
        /// Dùng khi customer book appointment, chọn xe xong sẽ hiện subscriptions active
        /// </summary>
        /// <param name="vehicleId">ID của vehicle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List active subscriptions cho vehicle đó</returns>
        Task<List<PackageSubscriptionSummaryDto>> GetActiveSubscriptionsForVehicleAsync(
            int vehicleId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra customer có subscription active cho vehicle không
        /// Dùng để validate trước khi purchase package mới
        /// </summary>
        /// <param name="customerId">ID của customer</param>
        /// <param name="vehicleId">ID của vehicle</param>
        /// <param name="packageId">ID của package (để check trùng)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu đã có subscription active cho package này</returns>
        Task<bool> HasActiveSubscriptionForPackageAsync(
            int customerId,
            int vehicleId,
            int packageId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy usage details của tất cả services trong subscription
        /// </summary>
        /// <param name="subscriptionId">ID của subscription</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List service usage details</returns>
        Task<List<PackageServiceUsageDto>> GetSubscriptionUsageDetailsAsync(
            int subscriptionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra subscription còn service nào available không
        /// </summary>
        /// <param name="subscriptionId">ID của subscription</param>
        /// <param name="serviceId">ID của service cần check</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu còn lượt sử dụng service này</returns>
        Task<bool> HasRemainingUsageForServiceAsync(
            int subscriptionId,
            int serviceId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra subscription có tồn tại không
        /// </summary>
        /// <param name="subscriptionId">ID của subscription</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu tồn tại</returns>
        Task<bool> SubscriptionExistsAsync(
            int subscriptionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check ownership: subscription này có thuộc customer này không
        /// Security check để đảm bảo customer chỉ xem được subscription của mình
        /// </summary>
        /// <param name="subscriptionId">ID của subscription</param>
        /// <param name="customerId">ID của customer</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu subscription thuộc customer</returns>
        Task<bool> IsSubscriptionOwnedByCustomerAsync(
            int subscriptionId,
            int customerId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// [SMART DEDUPLICATION] Lấy tất cả active subscriptions của customer cho vehicle cụ thể
        /// Dùng cho BuildAppointmentServicesAsync để tự động apply subscription
        ///
        /// Trả về CustomerPackageSubscription entities (KHÔNG phải DTO) với đầy đủ:
        /// - ServiceUsages collection (để check RemainingQuantity)
        /// - Package details (để lấy thông tin gói)
        /// - ExpiryDate, PurchaseDate (để calculate priority)
        ///
        /// Filter:
        /// - Status = Active
        /// - CustomerId = customerId
        /// - VehicleId = vehicleId
        /// - ExpiryDate > NOW (hoặc NULL)
        /// - Có ít nhất 1 service còn RemainingQuantity > 0
        /// </summary>
        /// <param name="customerId">ID của customer</param>
        /// <param name="vehicleId">ID của vehicle</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List CustomerPackageSubscription entities (với ServiceUsages included)</returns>
        Task<List<Core.Entities.CustomerPackageSubscription>> GetActiveSubscriptionsByCustomerAndVehicleAsync(
            int customerId,
            int vehicleId,
            CancellationToken cancellationToken = default);
    }
}
