using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;
using EVServiceCenter.Core.Enums;

namespace EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Services
{
    /// <summary>
    /// Service interface cho Package Subscription
    /// Chứa business logic, validation, wrap repository calls
    /// </summary>
    public interface IPackageSubscriptionService
    {
        // ========== QUERY METHODS ==========

        /// <summary>
        /// Lấy danh sách subscriptions của customer hiện tại
        /// </summary>
        Task<List<PackageSubscriptionSummaryDto>> GetMySubscriptionsAsync(
            int customerId,
            SubscriptionStatusEnum? statusFilter = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy chi tiết 1 subscription
        /// Security: Check ownership trước khi trả về
        /// </summary>
        Task<PackageSubscriptionResponseDto?> GetSubscriptionDetailsAsync(
            int subscriptionId,
            int requestingCustomerId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy subscriptions active cho vehicle (dùng khi book appointment)
        /// </summary>
        Task<List<PackageSubscriptionSummaryDto>> GetActiveSubscriptionsForVehicleAsync(
            int vehicleId,
            int customerId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy usage details của subscription
        /// </summary>
        Task<List<PackageServiceUsageDto>> GetSubscriptionUsageDetailsAsync(
            int subscriptionId,
            int requestingCustomerId,
            CancellationToken cancellationToken = default);

        // ========== COMMAND METHODS ==========

        /// <summary>
        /// Khách hàng mua/subscribe vào package
        /// Validate:
        /// - Package tồn tại và active
        /// - Vehicle thuộc customer
        /// - Chưa có subscription active cho package này trên xe này
        /// </summary>
        Task<PackageSubscriptionResponseDto> PurchasePackageAsync(
            PurchasePackageRequestDto request,
            int customerId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Hủy subscription
        /// Validate:
        /// - Subscription tồn tại và thuộc customer
        /// - Status phải là Active hoặc Suspended
        /// </summary>
        Task<bool> CancelSubscriptionAsync(
            int subscriptionId,
            string cancellationReason,
            int customerId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Tạm dừng subscription
        /// Customer có thể tạm dừng khi: xe đang sửa chữa, đi công tác
        /// Staff có thể tạm dừng khi: phát hiện gian lận
        /// Validate:
        /// - Subscription phải đang Active
        /// - Reason không được trống
        /// </summary>
        Task<bool> SuspendSubscriptionAsync(
            int subscriptionId,
            string reason,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Kích hoạt lại subscription đã bị Suspend
        /// Validate:
        /// - Subscription phải đang Suspended
        /// - Chưa hết hạn
        /// </summary>
        Task<bool> ReactivateSubscriptionAsync(
            int subscriptionId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Update service usage sau khi complete appointment
        /// INTERNAL USE - được gọi từ AppointmentService
        /// </summary>
        Task<bool> UpdateServiceUsageAfterAppointmentAsync(
            int subscriptionId,
            int serviceId,
            int quantityUsed,
            int appointmentId,
            CancellationToken cancellationToken = default);

        // ========== VALIDATION METHODS ==========

        /// <summary>
        /// Validate purchase request
        /// </summary>
        Task<(bool IsValid, string? ErrorMessage)> ValidatePurchaseRequestAsync(
            PurchasePackageRequestDto request,
            int customerId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check xem có thể sử dụng service từ subscription không
        /// </summary>
        Task<bool> CanUseServiceFromSubscriptionAsync(
            int subscriptionId,
            int serviceId,
            CancellationToken cancellationToken = default);
    }
}
