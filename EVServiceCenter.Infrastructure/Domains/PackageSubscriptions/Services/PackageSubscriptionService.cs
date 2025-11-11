using System;
using System.Linq;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Services;
using EVServiceCenter.Core.Enums;
using Microsoft.Extensions.Logging;
using EVServiceCenter.Core.Domains.Invoices.Interfaces;

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
        private readonly IInvoiceService _invoiceService;

        public PackageSubscriptionService(
            IPackageSubscriptionQueryRepository queryRepository,
            IPackageSubscriptionCommandRepository commandRepository,
            IMaintenancePackageQueryRepository packageQueryRepository,
            ILogger<PackageSubscriptionService> logger,
            IInvoiceService invoiceService)
        {
            _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
            _commandRepository = commandRepository ?? throw new ArgumentNullException(nameof(commandRepository));
            _packageQueryRepository = packageQueryRepository ?? throw new ArgumentNullException(nameof(packageQueryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _invoiceService = invoiceService ?? throw new ArgumentNullException(nameof(invoiceService));
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

        public async Task<List<ApplicableServiceDto>> GetApplicableServicesForVehicleAsync(
            int vehicleId,
            int customerId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var activeSubscriptions = await _queryRepository.GetActiveSubscriptionsByCustomerAndVehicleAsync(
                    customerId,
                    vehicleId,
                    cancellationToken);

                if (activeSubscriptions.Count == 0)
                {
                    _logger.LogDebug(
                        "No active subscriptions with remaining usage found for Customer {CustomerId}, Vehicle {VehicleId}",
                        customerId, vehicleId);
                    return new List<ApplicableServiceDto>();
                }

                var flattened = activeSubscriptions
                    .SelectMany(subscription => subscription.PackageServiceUsages
                        .Where(usage => usage.RemainingQuantity > 0)
                        .Select(usage => new ApplicableServiceDto
                        {
                            ServiceId = usage.ServiceId,
                            ServiceName = usage.Service?.ServiceName ?? $"Service #{usage.ServiceId}",
                            RemainingQuantity = usage.RemainingQuantity,
                            TotalQuantity = usage.TotalAllowedQuantity,
                            SubscriptionId = subscription.SubscriptionId,
                            SubscriptionCode = subscription.SubscriptionCode,
                            PackageId = subscription.PackageId,
                            PackageName = subscription.Package?.PackageName ?? string.Empty,
                            VehicleId = subscription.VehicleId ?? vehicleId,
                            ExpirationDate = subscription.ExpirationDate?.ToDateTime(TimeOnly.MinValue)
                        }))
                    .ToList();

                if (flattened.Count == 0)
                {
                    _logger.LogDebug(
                        "Active subscriptions exist but no remaining usages left (Customer {CustomerId}, Vehicle {VehicleId})",
                        customerId, vehicleId);
                    return flattened;
                }

                // UI only needs one record per serviceId – choose the subscription that expires soonest
                var bestPerService = flattened
                    .GroupBy(dto => dto.ServiceId)
                    .Select(group => group
                        .OrderBy(dto => dto.ExpirationDate ?? DateTime.MaxValue)
                        .ThenByDescending(dto => dto.RemainingQuantity)
                        .First())
                    .ToList();

                _logger.LogInformation(
                    "Found {Count} applicable services for Customer {CustomerId}, Vehicle {VehicleId}",
                    bestPerService.Count,
                    customerId,
                    vehicleId);

                return bestPerService;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting applicable services for Customer {CustomerId}, Vehicle {VehicleId}",
                    customerId,
                    vehicleId);
                throw;
            }
        }

        #endregion

        #region Command Methods

        public async Task<PackageSubscriptionResponseDto> PurchasePackageAsync(
            PurchasePackageRequestDto request,
            int customerId,
            int? createdByUserId,
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

                try
                {
                    var invoice = await _invoiceService.CreatePackageSubscriptionInvoiceAsync(
                        result.SubscriptionId,
                        result.CustomerId,
                        result.PackageName,
                        result.PricePaid,
                        createdByUserId,
                        cancellationToken);

                    var linked = await _commandRepository.UpdateInvoiceReferenceAsync(
                        result.SubscriptionId,
                        invoice.InvoiceId,
                        cancellationToken);

                    if (!linked)
                    {
                        _logger.LogWarning(
                            "Failed to link invoice {InvoiceId} to subscription {SubscriptionId}",
                            invoice.InvoiceId,
                            result.SubscriptionId);
                    }
                    else
                    {
                        result.InvoiceId = invoice.InvoiceId;
                        result.InvoiceCode = invoice.InvoiceCode;
                    }
                }
                catch (Exception invoiceEx)
                {
                    _logger.LogError(invoiceEx,
                        "Error creating invoice for subscription {SubscriptionId}",
                        result.SubscriptionId);
                    throw;
                }

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

        public async Task<bool> SuspendSubscriptionAsync(
            int subscriptionId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Suspending subscription {SubscriptionId}", subscriptionId);

                // ========== BUSINESS VALIDATION ==========
                var subscription = await _queryRepository.GetSubscriptionByIdAsync(
                    subscriptionId, cancellationToken);

                if (subscription == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy subscription {subscriptionId}");
                }

                if (subscription.Status != SubscriptionStatusEnum.Active)
                {
                    throw new InvalidOperationException(
                        $"Chỉ có thể tạm dừng subscription đang Active. " +
                        $"Trạng thái hiện tại: {subscription.StatusDisplayName}");
                }

                // Validate reason
                if (string.IsNullOrWhiteSpace(reason) || reason.Length < 10)
                {
                    throw new InvalidOperationException(
                        "Lý do tạm dừng phải có ít nhất 10 ký tự");
                }

                // ========== DELEGATE TO COMMAND REPOSITORY ==========
                return await _commandRepository.SuspendSubscriptionAsync(
                    subscriptionId, reason, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error suspending subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        public async Task<bool> ReactivateSubscriptionAsync(
            int subscriptionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Service: Reactivating subscription {SubscriptionId}", subscriptionId);

                // ========== BUSINESS VALIDATION ==========
                var subscription = await _queryRepository.GetSubscriptionByIdAsync(
                    subscriptionId, cancellationToken);

                if (subscription == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy subscription {subscriptionId}");
                }

                if (subscription.Status != SubscriptionStatusEnum.Suspended)
                {
                    throw new InvalidOperationException(
                        $"Chỉ có thể kích hoạt lại subscription đang Suspended. " +
                        $"Trạng thái hiện tại: {subscription.StatusDisplayName}");
                }

                // Check expiry (không cho reactivate subscription đã hết hạn)
                if (subscription.ExpiryDate.HasValue && 
                    subscription.ExpiryDate.Value < DateTime.UtcNow)
                {
                    throw new InvalidOperationException(
                        $"Không thể kích hoạt lại subscription đã hết hạn vào {subscription.ExpiryDate.Value:dd/MM/yyyy}. " +
                        "Vui lòng mua gói mới.");
                }

                // ========== DELEGATE TO COMMAND REPOSITORY ==========
                return await _commandRepository.ReactivateSubscriptionAsync(
                    subscriptionId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service: Error reactivating subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        /// <summary>
        /// 💰 [STAFF ONLY] Xác nhận thanh toán Cash/BankTransfer
        /// Chuyển subscription từ PendingPayment → Active
        /// </summary>
        public async Task<bool> ConfirmPaymentAsync(
            ConfirmPaymentRequestDto request,
            int staffUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "💰 Staff {StaffId} confirming payment for subscription {SubscriptionId} via {PaymentMethod}",
                    staffUserId, request.SubscriptionId, request.PaymentMethod);

                // ========== BUSINESS VALIDATION ==========
                var subscription = await _queryRepository.GetSubscriptionByIdAsync(
                    request.SubscriptionId, cancellationToken);

                if (subscription == null)
                {
                    throw new InvalidOperationException($"Không tìm thấy subscription {request.SubscriptionId}");
                }

                // Validate status
                if (subscription.Status != SubscriptionStatusEnum.PendingPayment)
                {
                    throw new InvalidOperationException(
                        $"Chỉ có thể confirm payment cho subscription đang PendingPayment. " +
                        $"Trạng thái hiện tại: {subscription.StatusDisplayName}");
                }

                // Validate payment amount
                var expectedAmount = subscription.PricePaid;
                if (request.PaidAmount < expectedAmount)
                {
                    throw new InvalidOperationException(
                        $"Số tiền thanh toán ({request.PaidAmount:N0}đ) không đủ. Cần {expectedAmount:N0}đ");
                }

                // Validate payment method
                if (request.PaymentMethod != "Cash" && request.PaymentMethod != "BankTransfer")
                {
                    throw new InvalidOperationException(
                        "PaymentMethod phải là 'Cash' hoặc 'BankTransfer'");
                }

                // Validate BankTransfer fields
                if (request.PaymentMethod == "BankTransfer")
                {
                    if (string.IsNullOrWhiteSpace(request.BankTransactionId))
                    {
                        throw new InvalidOperationException(
                            "Mã giao dịch ngân hàng là bắt buộc khi thanh toán qua BankTransfer");
                    }

                    if (!request.TransferDate.HasValue)
                    {
                        throw new InvalidOperationException(
                            "Ngày chuyển khoản là bắt buộc khi thanh toán qua BankTransfer");
                    }
                }

                // ========== DELEGATE TO COMMAND REPOSITORY ==========
                var result = await _commandRepository.ConfirmPaymentAsync(
                    request, staffUserId, cancellationToken);

                _logger.LogInformation(
                    "✅ Payment confirmed for subscription {SubscriptionId}: {Amount}đ via {Method}",
                    request.SubscriptionId, request.PaidAmount, request.PaymentMethod);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "❌ Error confirming payment for subscription {SubscriptionId}",
                    request.SubscriptionId);
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
