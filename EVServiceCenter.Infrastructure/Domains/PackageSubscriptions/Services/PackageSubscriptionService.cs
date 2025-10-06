using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Services;
using EVServiceCenter.Core.Enums;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Services
{
    /// <summary>
    /// Service Implementation cho Package Subscription
    /// Chứa business logic, validation, orchestrate repository calls
    /// </summary>
    public class PackageSubscriptionService : IPackageSubscriptionService
    {
        private readonly IPackageSubscriptionQueryRepository _queryRepository;
        private readonly IPackageSubscriptionCommandRepository _commandRepository;
        private readonly IMaintenancePackageQueryRepository _packageQueryRepository;
        private readonly ILogger<PackageSubscriptionService> _logger;

        public PackageSubscriptionService(
            IPackageSubscriptionQueryRepository queryRepository,
            IPackageSubscriptionCommandRepository commandRepository,
            IMaintenancePackageQueryRepository packageQueryRepository,
            ILogger<PackageSubscriptionService> logger)
        {
            _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
            _commandRepository = commandRepository ?? throw new ArgumentNullException(nameof(commandRepository));
            _packageQueryRepository = packageQueryRepository ?? throw new ArgumentNullException(nameof(packageQueryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Query Methods

        public async Task<List<PackageSubscriptionSummaryDto>> GetMySubscriptionsAsync(
            int customerId,
            SubscriptionStatusEnum? statusFilter = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Getting subscriptions for customer {CustomerId}", customerId);
                return await _queryRepository.GetCustomerSubscriptionsAsync(
                    customerId, statusFilter, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions for customer {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<PackageSubscriptionResponseDto?> GetSubscriptionDetailsAsync(
            int subscriptionId,
            int requestingCustomerId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // ========== SECURITY CHECK: OWNERSHIP ==========
                var isOwner = await _queryRepository.IsSubscriptionOwnedByCustomerAsync(
                    subscriptionId, requestingCustomerId, cancellationToken);

                if (!isOwner)
                {
                    _logger.LogWarning(
                        "Customer {CustomerId} attempted to access subscription {SubscriptionId} without ownership",
                        requestingCustomerId, subscriptionId);
                    throw new UnauthorizedAccessException("Bạn không có quyền xem subscription này");
                }

                return await _queryRepository.GetSubscriptionByIdAsync(subscriptionId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription details {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<List<PackageSubscriptionSummaryDto>> GetActiveSubscriptionsForVehicleAsync(
            int vehicleId,
            int customerId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // TODO: Add vehicle ownership check if needed
                return await _queryRepository.GetActiveSubscriptionsForVehicleAsync(
                    vehicleId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active subscriptions for vehicle {VehicleId}", vehicleId);
                throw;
            }
        }

        public async Task<List<PackageServiceUsageDto>> GetSubscriptionUsageDetailsAsync(
            int subscriptionId,
            int requestingCustomerId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // ========== SECURITY CHECK: OWNERSHIP ==========
                var isOwner = await _queryRepository.IsSubscriptionOwnedByCustomerAsync(
                    subscriptionId, requestingCustomerId, cancellationToken);

                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền xem usage của subscription này");
                }

                return await _queryRepository.GetSubscriptionUsageDetailsAsync(
                    subscriptionId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage details for subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        #endregion

        #region Command Methods

        public async Task<PackageSubscriptionResponseDto> PurchasePackageAsync(
            PurchasePackageRequestDto request,
            int customerId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Customer {CustomerId} purchasing package {PackageId}",
                    customerId, request.PackageId);

                // ========== BUSINESS VALIDATION ==========
                var (isValid, errorMessage) = await ValidatePurchaseRequestAsync(
                    request, customerId, cancellationToken);

                if (!isValid)
                {
                    throw new InvalidOperationException($"Validation failed: {errorMessage}");
                }

                // ========== DELEGATE TO COMMAND REPOSITORY ==========
                var result = await _commandRepository.PurchasePackageAsync(
                    request, customerId, cancellationToken);

                _logger.LogInformation("Service: Successfully purchased subscription {SubscriptionId}",
                    result.SubscriptionId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error purchasing package {PackageId}", request.PackageId);
                throw;
            }
        }

        public async Task<bool> CancelSubscriptionAsync(
            int subscriptionId,
            string cancellationReason,
            int customerId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Customer {CustomerId} cancelling subscription {SubscriptionId}",
                    customerId, subscriptionId);

                // ========== SECURITY CHECK: OWNERSHIP ==========
                var isOwner = await _queryRepository.IsSubscriptionOwnedByCustomerAsync(
                    subscriptionId, customerId, cancellationToken);

                if (!isOwner)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền hủy subscription này");
                }

                // ========== BUSINESS VALIDATION ==========
                var subscription = await _queryRepository.GetSubscriptionByIdAsync(
                    subscriptionId, cancellationToken);

                if (subscription == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy subscription {subscriptionId}");
                }

                if (subscription.Status != SubscriptionStatusEnum.Active &&
                    subscription.Status != SubscriptionStatusEnum.Suspended)
                {
                    throw new InvalidOperationException(
                        "Chỉ có thể hủy subscription đang Active hoặc Suspended");
                }

                // ========== DELEGATE TO COMMAND REPOSITORY ==========
                return await _commandRepository.CancelSubscriptionAsync(
                    subscriptionId, cancellationReason, customerId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error cancelling subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<bool> UpdateServiceUsageAfterAppointmentAsync(
            int subscriptionId,
            int serviceId,
            int quantityUsed,
            int appointmentId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Service: Updating usage for subscription {SubscriptionId}, service {ServiceId}",
                    subscriptionId, serviceId);

                // Validate subscription exists and has remaining usage
                var hasRemaining = await _queryRepository.HasRemainingUsageForServiceAsync(
                    subscriptionId, serviceId, cancellationToken);

                if (!hasRemaining)
                {
                    throw new InvalidOperationException(
                        "Subscription không còn lượt sử dụng cho dịch vụ này");
                }

                return await _commandRepository.UpdateServiceUsageAsync(
                    subscriptionId, serviceId, quantityUsed, appointmentId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Service: Error updating usage for subscription {SubscriptionId}, service {ServiceId}",
                    subscriptionId, serviceId);
                throw;
            }
        }

        #endregion

        #region Validation Methods

        public async Task<(bool IsValid, string? ErrorMessage)> ValidatePurchaseRequestAsync(
            PurchasePackageRequestDto request,
            int customerId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // ========== VALIDATE PACKAGE ==========
                var package = await _packageQueryRepository.GetPackageByIdAsync(
                    request.PackageId, cancellationToken);

                if (package == null)
                {
                    return (false, $"Không tìm thấy gói dịch vụ với ID: {request.PackageId}");
                }

                if (package.Status != PackageStatusEnum.Active)
                {
                    return (false, "Gói dịch vụ này hiện không còn hoạt động");
                }

                // ========== VALIDATE DUPLICATE SUBSCRIPTION ==========
                var hasActive = await _queryRepository.HasActiveSubscriptionForPackageAsync(
                    customerId, request.VehicleId, request.PackageId, cancellationToken);

                if (hasActive)
                {
                    return (false,
                        "Bạn đã có subscription active cho gói này trên xe này. Vui lòng chờ hết hạn hoặc hủy subscription cũ.");
                }

                // ========== VALIDATE PAYMENT ==========
                if (request.AmountPaid < package.TotalPriceAfterDiscount)
                {
                    return (false,
                        $"Số tiền thanh toán ({request.AmountPaid:N0} VNĐ) không đủ. Cần {package.TotalPriceAfterDiscount:N0} VNĐ");
                }

                if (string.IsNullOrWhiteSpace(request.PaymentMethod))
                {
                    return (false, "Phương thức thanh toán không được để trống");
                }

                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error validating purchase request");
                throw;
            }
        }

        public async Task<bool> CanUseServiceFromSubscriptionAsync(
            int subscriptionId,
            int serviceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _queryRepository.HasRemainingUsageForServiceAsync(
                    subscriptionId, serviceId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Service: Error checking can use service {ServiceId} from subscription {SubscriptionId}",
                    serviceId, subscriptionId);
                throw;
            }
        }

        #endregion
    }
}
