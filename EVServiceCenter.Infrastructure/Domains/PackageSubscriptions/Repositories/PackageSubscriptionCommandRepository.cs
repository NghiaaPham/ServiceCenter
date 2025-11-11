using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Repositories
{
    /// <summary>
    /// Command Repository Implementation cho Package Subscription
    /// Chứa tất cả WRITE operations - Create, Update, Cancel
    /// Thay đổi state của database
    /// </summary>
    public class PackageSubscriptionCommandRepository : IPackageSubscriptionCommandRepository
    {
        private readonly EVDbContext _context;
        private readonly IPackageSubscriptionQueryRepository _queryRepository;
        private readonly IMaintenancePackageQueryRepository _packageQueryRepository;
        private readonly ILogger<PackageSubscriptionCommandRepository> _logger;

        public PackageSubscriptionCommandRepository(
            EVDbContext context,
            IPackageSubscriptionQueryRepository queryRepository,
            IMaintenancePackageQueryRepository packageQueryRepository,
            ILogger<PackageSubscriptionCommandRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));
            _packageQueryRepository = packageQueryRepository ?? throw new ArgumentNullException(nameof(packageQueryRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region PurchasePackageAsync

        public async Task<PackageSubscriptionResponseDto> PurchasePackageAsync(
            PurchasePackageRequestDto request,
            int customerId,
            CancellationToken cancellationToken = default)
        {
            // ✅ FIX: Use ExecutionStrategy to support retry with transaction
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    _logger.LogInformation("Customer {CustomerId} purchasing package {PackageId}",
                        customerId, request.PackageId);

                    // ========== VALIDATE PACKAGE EXISTS ==========
                    var package = await _packageQueryRepository.GetPackageByIdAsync(
                        request.PackageId, cancellationToken);

                    if (package == null)
                    {
                        throw new InvalidOperationException($"Không tìm thấy gói dịch vụ với ID: {request.PackageId}");
                    }

                    if (package.Status != PackageStatusEnum.Active)
                    {
                        throw new InvalidOperationException("Gói dịch vụ này hiện không còn hoạt động");
                    }

                    // ========== CHECK DUPLICATE SUBSCRIPTION ==========
                    var hasActiveSubscription = await _queryRepository.HasActiveSubscriptionForPackageAsync(
                        customerId, request.VehicleId, request.PackageId, cancellationToken);

                    if (hasActiveSubscription)
                    {
                        throw new InvalidOperationException(
                            "Bạn đã có subscription active cho gói này trên xe này rồi. Vui lòng chờ hết hạn hoặc hủy subscription cũ trước.");
                    }

                    // ========== GET VEHICLE INFO (for mileage tracking) ==========
                    var vehicle = await _context.CustomerVehicles
                        .FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId, cancellationToken);

                    if (vehicle == null)
                    {
                        throw new InvalidOperationException($"Không tìm thấy xe với ID: {request.VehicleId}");
                    }

                    // ═══════════════════════════════════════════════════════════
                    // ✅ PHASE 2: PACKAGE PURCHASE DISCOUNT CALCULATION
                    // ═══════════════════════════════════════════════════════════

                    // 1️⃣ Get prices from Package
                    decimal originalPrice = package.OriginalPriceBeforeDiscount;
                    decimal finalPrice = package.TotalPriceAfterDiscount;

                    // 2️⃣ Get DiscountPercent from Package (nếu package có discount)
                    decimal discountPercent = package.DiscountPercent ?? 0;

                    // 3️⃣ Calculate DiscountAmount (VNĐ)
                    decimal discountAmount = originalPrice - finalPrice;

                    // 5️⃣ Log discount information
                    _logger.LogInformation(
                        "💰 Package Purchase Discount: PackageId={PackageId}, " +
                        "OriginalPrice={Original}đ, DiscountPercent={Percent}%, " +
                        "DiscountAmount={Discount}đ, FinalPrice={Final}đ",
                        package.PackageId, originalPrice, discountPercent,
                        discountAmount, finalPrice);

                    // ═══════════════════════════════════════════════════════════

                    // ========== CREATE SUBSCRIPTION ENTITY ==========
                    var purchaseDate = DateTime.UtcNow;
                    var startDate = DateOnly.FromDateTime(purchaseDate);
                    DateOnly? expirationDate = null;

                    if (package.ValidityPeriodInDays.HasValue)
                    {
                        expirationDate = startDate.AddDays(package.ValidityPeriodInDays.Value);
                    }

                    var subscriptionCode = $"SUB-{customerId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

                    var subscription = new CustomerPackageSubscription
                    {
                        SubscriptionCode = subscriptionCode,
                        CustomerId = customerId,
                        PackageId = request.PackageId,
                        VehicleId = request.VehicleId,
                        PurchaseDate = purchaseDate,
                        StartDate = startDate,
                        ExpirationDate = expirationDate,
                        InitialVehicleMileage = vehicle.Mileage,
                        // ✅ PHASE 2: Set pricing fields với discount
                        OriginalPrice = originalPrice,
                        DiscountPercent = discountPercent,
                        DiscountAmount = discountAmount,
                        PaymentAmount = finalPrice, // FinalPrice sau khi giảm giá
                        // ✅ FIX: Status = PendingPayment instead of Active
                        // Chỉ chuyển sang Active sau khi thanh toán thành công
                        Status = SubscriptionStatusEnum.PendingPayment.ToString(),
                        Notes = string.IsNullOrWhiteSpace(request.CustomerNotes)
                            ? null
                            : request.CustomerNotes.Trim()
                    };

                    _context.CustomerPackageSubscriptions.Add(subscription);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Created subscription {SubscriptionId} with Status=PendingPayment for customer {CustomerId}",
                        subscription.SubscriptionId, customerId);

                    // ========== CREATE PACKAGE SERVICE USAGES ==========
                    var serviceUsages = package.IncludedServices.Select(svc => new PackageServiceUsage
                    {
                        SubscriptionId = subscription.SubscriptionId,
                        ServiceId = svc.ServiceId,
                        TotalAllowedQuantity = svc.QuantityInPackage,
                        UsedQuantity = 0,
                        RemainingQuantity = svc.QuantityInPackage,
                        LastUsedDate = null,
                        LastUsedAppointmentId = null
                    }).ToList();

                    _context.PackageServiceUsages.AddRange(serviceUsages);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Created {Count} service usages for subscription {SubscriptionId}",
                        serviceUsages.Count, subscription.SubscriptionId);

                    await transaction.CommitAsync(cancellationToken);

                    // ========== RETURN FULL DETAILS ==========
                    var result = await _queryRepository.GetSubscriptionByIdAsync(
                        subscription.SubscriptionId, cancellationToken);

                    if (result == null)
                    {
                        throw new InvalidOperationException("Failed to retrieve created subscription");
                    }

                    _logger.LogInformation("Successfully purchased subscription {SubscriptionId} - {PackageName} (Status: PendingPayment)",
                        result.SubscriptionId, result.PackageName);

                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error purchasing package {PackageId} for customer {CustomerId}",
                        request.PackageId, customerId);
                    throw;
                }
            });
        }

        public async Task<bool> UpdateInvoiceReferenceAsync(
            int subscriptionId,
            int invoiceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var affected = await _context.CustomerPackageSubscriptions
                    .Where(s => s.SubscriptionId == subscriptionId)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(s => s.InvoiceId, invoiceId),
                        cancellationToken);

                return affected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating invoice reference for subscription {SubscriptionId}",
                    subscriptionId);
                throw;
            }
        }

        #endregion

        #region UpdateServiceUsageAsync - With Pessimistic Lock

        /// <summary>
        /// ✅ ADVANCED: Update service usage với PESSIMISTIC LOCK (UPDLOCK, ROWLOCK)
        ///
        /// RACE CONDITION HANDLING:
        /// - Dùng SQL UPDLOCK, ROWLOCK để lock row khi SELECT
        /// - Ngăn 2 transactions cùng đọc RemainingQuantity = 1 và cùng trừ
        /// - Transaction A lock row trước → Trừ thành công
        /// - Transaction B đợi lock release → Đọc RemainingQuantity = 0 → Return FALSE
        ///
        /// IMPORTANT:
        /// - Method này KHÔNG throw exception khi hết lượt
        /// - Return TRUE nếu trừ thành công, FALSE nếu không đủ lượt
        /// - Caller (CompleteAppointmentAsync) sẽ handle graceful degradation
        /// </summary>
        /// <param name="subscriptionId">ID của subscription cần trừ lượt</param>
        /// <param name="serviceId">ID của service cần trừ lượt</param>
        /// <param name="quantityUsed">Số lượt cần trừ (thường là 1)</param>
        /// <param name="appointmentId">ID của appointment đang sử dụng service này</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu trừ lượt thành công, FALSE nếu không đủ lượt hoặc không tìm thấy</returns>

        public async Task<bool> UpdateServiceUsageAsync(
          int subscriptionId,
          int serviceId,
          int quantityUsed,
          int appointmentId,
          CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    ">>> [UpdateServiceUsageAsync] START. DbContextHash={DbHash}, SubscriptionId={SubscriptionId}, ServiceId={ServiceId}, Quantity={Quantity}, AppointmentId={AppointmentId}",
                    _context.GetHashCode(), subscriptionId, serviceId, quantityUsed, appointmentId);

                // ✅ STEP 0: Validate subscription status & expiry
                var subscription = await _context.CustomerPackageSubscriptions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);

                if (subscription == null)
                {
                    _logger.LogWarning(
                        "⚠️ Subscription {SubscriptionId} not found → cannot deduct usage",
                        subscriptionId);
                    return false;
                }

                _logger.LogInformation(
                    "📄 Subscription loaded: SubscriptionId={SubscriptionId}, Status={Status}, ExpirationDate={Expiry}",
                    subscription.SubscriptionId,
                    subscription.Status,
                    subscription.ExpirationDate);

                if (!string.Equals(subscription.Status, SubscriptionStatusEnum.Active.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "⚠️ Subscription {SubscriptionId} is not Active (Status={Status}) → cannot deduct usage",
                        subscriptionId, subscription.Status);
                    return false;
                }

                if (subscription.ExpirationDate.HasValue &&
                    subscription.ExpirationDate.Value < DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    _logger.LogWarning(
                        "⚠️ Subscription {SubscriptionId} has expired at {ExpiryDate} → cannot deduct usage",
                        subscriptionId, subscription.ExpirationDate.Value);
                    return false;
                }

                // 🔒 STEP 1: SELECT with UPDLOCK + ROWLOCK – lock single row
                var sql = @"
            SELECT UsageID, SubscriptionID, ServiceID,
                   TotalAllowedQuantity, UsedQuantity, RemainingQuantity,
                   LastUsedDate, LastUsedAppointmentID, Notes
            FROM PackageServiceUsages WITH (UPDLOCK, ROWLOCK)
            WHERE SubscriptionID = {0} AND ServiceID = {1}";

                _logger.LogInformation(
                    "🔒 Executing usage select with lock for SubscriptionId={SubscriptionId}, ServiceId={ServiceId}",
                    subscriptionId, serviceId);

                var usage = await _context.PackageServiceUsages
                    .FromSqlRaw(sql, subscriptionId, serviceId)
                    .AsTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                if (usage == null)
                {
                    _logger.LogWarning(
                        "⚠️ Service usage NOT FOUND: SubscriptionId={SubscriptionId}, ServiceId={ServiceId} → return FALSE",
                        subscriptionId, serviceId);
                    return false;
                }

                _logger.LogInformation(
                    "📄 Current usage row BEFORE deduction: UsageId={UsageId}, SubscriptionId={SubId}, ServiceId={ServiceId}, Total={Total}, Used={Used}, Remaining={Remaining}, LastUsedAppointmentId={LastAppId}, LastUsedDate={LastUsedDate}",
                    usage.UsageId,
                    usage.SubscriptionId,
                    usage.ServiceId,
                    usage.TotalAllowedQuantity,
                    usage.UsedQuantity,
                    usage.RemainingQuantity,
                    usage.LastUsedAppointmentId,
                    usage.LastUsedDate);

                // 🔎 STEP 2: Check remaining quantity
                var remainingBefore = usage.RemainingQuantity;
                _logger.LogInformation(
                    "🔍 Locked usage row: UsageId={UsageId}, RemainingQuantity={Remaining}, QuantityToDeduct={ToDeduct}",
                    usage.UsageId, remainingBefore, quantityUsed);

                if (remainingBefore < quantityUsed)
                {
                    _logger.LogWarning(
                        "⚠️ INSUFFICIENT USAGE: SubscriptionId={SubscriptionId}, ServiceId={ServiceId}, " +
                        "Remaining={Remaining}, Required={Required} → return FALSE (graceful degradation)",
                        subscriptionId, serviceId, remainingBefore, quantityUsed);

                    return false;
                }

                // ✏️ STEP 3: Prepare update – EF tracking
                var usedBefore = usage.UsedQuantity;
                var usedAfter = usedBefore + quantityUsed;
                var remainingAfter = remainingBefore - quantityUsed;

                usage.UsedQuantity = usedAfter;
                usage.RemainingQuantity = remainingAfter;
                usage.LastUsedDate = DateTime.UtcNow;
                usage.LastUsedAppointmentId = appointmentId;

                _logger.LogInformation(
                    "✏️ Prepared usage update: UsageId={UsageId}, Used: {OldUsed} → {NewUsed}, Remaining: {OldRemaining} → {NewRemaining}, LastUsedAppointmentId={LastAppId}",
                    usage.UsageId, usedBefore, usedAfter, remainingBefore, remainingAfter, appointmentId);

                _logger.LogInformation(
                    "🔎 EF ChangeTracker state for usage entity: {State}",
                    _context.Entry(usage).State);

                // ❗ Tạm thời vẫn để outer transaction SaveChanges – mục tiêu test log trước
                _logger.LogInformation(
                    "⏹ [UpdateServiceUsageAsync] END (no SaveChanges here). Returning TRUE for UsageId={UsageId}",
                    usage.UsageId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ ERROR updating service usage: SubscriptionId={SubscriptionId}, ServiceId={ServiceId}, AppointmentId={AppointmentId}",
                    subscriptionId, serviceId, appointmentId);
                throw;
            }
        }


        #endregion

        #region RefundServiceUsageAsync - Compensation Logic

        /// <summary>
        /// 🔧 FIX GAP #6: Refund/compensate subscription usage
        /// Called when deduction was successful but appointment completion failed
        /// Ensures no usage is lost due to partial failures
        /// </summary>
        /// <param name="subscriptionId">ID của subscription cần refund</param>
        /// <param name="serviceId">ID của service cần refund</param>
        /// <param name="quantityToRefund">Số lượt cần hoàn lại (thường là 1)</param>
        /// <param name="reason">Lý do refund (cho audit)</param>
        /// <param name="appointmentId">ID của appointment gây ra refund</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TRUE nếu refund thành công, FALSE nếu không tìm thấy</returns>
        public async Task<bool> RefundServiceUsageAsync(
            int subscriptionId,
            int serviceId,
            int quantityToRefund,
            string reason,
            int? appointmentId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogWarning(
                    "🔄 [COMPENSATION] Refunding usage: SubscriptionId={SubscriptionId}, " +
                    "ServiceId={ServiceId}, QuantityToRefund={Quantity}, " +
                    "Reason={Reason}, AppointmentId={AppointmentId}",
                    subscriptionId, serviceId, quantityToRefund, reason, appointmentId);

                // Find usage record
                var usage = await _context.PackageServiceUsages
                    .FirstOrDefaultAsync(u =>
                        u.SubscriptionId == subscriptionId &&
                        u.ServiceId == serviceId,
                        cancellationToken);

                if (usage == null)
                {
                    _logger.LogError(
                        "❌ Cannot refund: Usage record NOT FOUND for SubscriptionId={SubscriptionId}, ServiceId={ServiceId}",
                        subscriptionId, serviceId);
                    return false;
                }

                // Validate refund amount
                var totalAllowed = usage.TotalAllowedQuantity;
                var currentUsed = usage.UsedQuantity;
                var currentRemaining = usage.RemainingQuantity;

                if (currentUsed < quantityToRefund)
                {
                    _logger.LogWarning(
                        "⚠️ Refund quantity ({Refund}) exceeds used quantity ({Used}). " +
                        "Adjusting refund to {Used}",
                        quantityToRefund, currentUsed, currentUsed);
                    quantityToRefund = currentUsed;
                }

                // Apply refund
                var newUsed = currentUsed - quantityToRefund;
                var newRemaining = currentRemaining + quantityToRefund;

                // Ensure we don't exceed total allowed
                if (newRemaining > totalAllowed)
                {
                    _logger.LogWarning(
                        "⚠️ Refund would exceed total allowed ({Total}). Capping at total allowed.",
                        totalAllowed);
                    newRemaining = totalAllowed;
                    newUsed = totalAllowed - newRemaining;
                }

                var oldUsed = usage.UsedQuantity;
                var oldRemaining = usage.RemainingQuantity;

                usage.UsedQuantity = newUsed;
                usage.RemainingQuantity = newRemaining;
                usage.Notes = $"[REFUND] {reason} | Previous: Used={oldUsed}, Remaining={oldRemaining}";

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "✅ Usage refunded successfully: SubscriptionId={SubscriptionId}, ServiceId={ServiceId}. " +
                    "UsedQuantity: {OldUsed} → {NewUsed}, RemainingQuantity: {OldRemaining} → {NewRemaining}",
                    subscriptionId, serviceId, oldUsed, newUsed, oldRemaining, newRemaining);

                // Update subscription status if needed
                await CheckAndUpdateFullyUsedStatusAsync(subscriptionId, cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ ERROR refunding service usage: SubscriptionId={SubscriptionId}, ServiceId={ServiceId}",
                    subscriptionId, serviceId);
                return false; // Don't throw - this is compensation, best effort
            }
        }

        #endregion

        #region CancelSubscriptionAsync

        public async Task<bool> CancelSubscriptionAsync(
            int subscriptionId,
            string cancellationReason,
            int cancelledByUserId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Cancelling subscription {SubscriptionId} by user {UserId}",
                    subscriptionId, cancelledByUserId);

                var subscription = await _context.CustomerPackageSubscriptions
                    .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);

                if (subscription == null)
                {
                    _logger.LogWarning("Subscription not found: {SubscriptionId}", subscriptionId);
                    return false;
                }

                subscription.Status = SubscriptionStatusEnum.Cancelled.ToString();
                subscription.CancellationReason = cancellationReason.Trim();
                subscription.CancelledDate = DateOnly.FromDateTime(DateTime.UtcNow);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully cancelled subscription {SubscriptionId}", subscriptionId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        #endregion

        #region SuspendSubscriptionAsync

        public async Task<bool> SuspendSubscriptionAsync(
            int subscriptionId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Suspending subscription {SubscriptionId}", subscriptionId);

                var subscription = await _context.CustomerPackageSubscriptions
                    .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);

                if (subscription == null)
                {
                    _logger.LogWarning("Subscription not found: {SubscriptionId}", subscriptionId);
                    return false;
                }

                subscription.Status = SubscriptionStatusEnum.Suspended.ToString();
                subscription.CancellationReason = reason.Trim(); // Reuse field for suspend reason

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully suspended subscription {SubscriptionId}", subscriptionId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        #endregion

        #region ReactivateSubscriptionAsync

        public async Task<bool> ReactivateSubscriptionAsync(
            int subscriptionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Reactivating subscription {SubscriptionId}", subscriptionId);

                var subscription = await _context.CustomerPackageSubscriptions
                    .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);

                if (subscription == null)
                {
                    _logger.LogWarning("Subscription not found: {SubscriptionId}", subscriptionId);
                    return false;
                }

                if (!string.Equals(subscription.Status, SubscriptionStatusEnum.Suspended.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Only Suspended subscriptions can be reactivated.");
                }

                subscription.Status = SubscriptionStatusEnum.Active.ToString();
                subscription.PurchaseDate ??= DateTime.UtcNow;
                subscription.CancelledDate = null;
                subscription.CancellationReason = null;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Successfully reactivated subscription {SubscriptionId}", subscriptionId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }
        #endregion

        #region AutoUpdateExpiredSubscriptionsAsync

        public async Task<int> AutoUpdateExpiredSubscriptionsAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Auto-updating expired subscriptions");

                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                var activeStatus = SubscriptionStatusEnum.Active.ToString();
                var expiredStatus = SubscriptionStatusEnum.Expired.ToString();
                var fullyUsedStatus = SubscriptionStatusEnum.FullyUsed.ToString();

                // Get active subscriptions that are expired by date
                var expiredByDate = await _context.CustomerPackageSubscriptions
                    .Where(s =>
                        s.Status == activeStatus &&
                        s.ExpirationDate.HasValue &&
                        s.ExpirationDate.Value < today)
                    .ToListAsync(cancellationToken);

                foreach (var subscription in expiredByDate)
                {
                    subscription.Status = expiredStatus;
                }

                // Get active subscriptions that are fully used
                var activeSubscriptions = await _context.CustomerPackageSubscriptions
                    .Where(s => s.Status == activeStatus)
                    .Include(s => s.PackageServiceUsages)
                    .ToListAsync(cancellationToken);

                var fullyUsedSubscriptions = activeSubscriptions
                    .Where(s => s.PackageServiceUsages.All(u => u.RemainingQuantity == 0))
                    .ToList();

                foreach (var subscription in fullyUsedSubscriptions)
                {
                    subscription.Status = fullyUsedStatus;
                }

                var totalUpdated = expiredByDate.Count + fullyUsedSubscriptions.Count;

                if (totalUpdated > 0)
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }

                _logger.LogInformation(
                    "Auto-update complete: {ExpiredCount} expired by date, {FullyUsedCount} fully used",
                    expiredByDate.Count, fullyUsedSubscriptions.Count);

                return totalUpdated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-updating expired subscriptions");
                throw;
            }
        }

        #endregion

        #region ConfirmPaymentAsync

        /// <summary>
        /// [STAFF ONLY] Confirm Cash/BankTransfer payments and move subscription to Active.
        /// </summary>
        public async Task<bool> ConfirmPaymentAsync(
            ConfirmPaymentRequestDto request,
            int staffUserId,
            CancellationToken cancellationToken = default)
        {
            var executionStrategy = _context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    _logger.LogInformation(
                        "Staff {StaffId} confirming payment for subscription {SubscriptionId}",
                        staffUserId, request.SubscriptionId);

                    var subscription = await _context.CustomerPackageSubscriptions
                        .FirstOrDefaultAsync(s => s.SubscriptionId == request.SubscriptionId, cancellationToken);

                    if (subscription == null)
                    {
                        throw new InvalidOperationException($"Subscription {request.SubscriptionId} not found");
                    }

                    subscription.Status = SubscriptionStatusEnum.Active.ToString();
                    subscription.PurchaseDate ??= DateTime.UtcNow;

                    if (subscription.InvoiceId.HasValue)
                    {
                        var invoice = await _context.Invoices
                            .FirstOrDefaultAsync(i => i.InvoiceId == subscription.InvoiceId.Value, cancellationToken);

                        if (invoice != null)
                        {
                            invoice.PaidAmount = (invoice.PaidAmount ?? 0) + request.PaidAmount;
                            invoice.OutstandingAmount = (invoice.GrandTotal ?? 0) - invoice.PaidAmount;
                            invoice.Status = invoice.OutstandingAmount <= 0 ? "Paid" : "PartiallyPaid";
                            invoice.UpdatedDate = DateTime.UtcNow;
                            invoice.UpdatedBy = staffUserId;
                        }
                        else
                        {
                            _logger.LogWarning(
                                "Invoice {InvoiceId} not found when confirming payment for subscription {SubscriptionId}",
                                subscription.InvoiceId.Value,
                                subscription.SubscriptionId);
                        }
                    }

                    var paymentNote = $"Thanh toan {request.PaymentMethod}: {request.PaidAmount:N0}d " +
                                      $"xac nhan boi Staff #{staffUserId} luc {DateTime.UtcNow:dd/MM/yyyy HH:mm}";

                    if (request.PaymentMethod == "BankTransfer")
                    {
                        paymentNote += $"\nMa GD: {request.BankTransactionId}, Ngay: {request.TransferDate:dd/MM/yyyy}";
                    }

                    if (!string.IsNullOrWhiteSpace(request.Notes))
                    {
                        paymentNote += $"\nGhi chu: {request.Notes}";
                    }

                    subscription.Notes = string.IsNullOrWhiteSpace(subscription.Notes)
                        ? paymentNote
                        : subscription.Notes + "\n---\n" + paymentNote;

                    await _context.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation(
                        "Payment confirmed successfully: Subscription {SubscriptionId} is Active",
                        request.SubscriptionId);

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex,
                        "Error confirming payment for subscription {SubscriptionId}",
                        request.SubscriptionId);
                    throw;
                }
            });
        }
        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Check nếu tất cả services đã dùng hết thì update status = FullyUsed
        /// </summary>
        private async Task CheckAndUpdateFullyUsedStatusAsync(
            int subscriptionId,
            CancellationToken cancellationToken)
        {
            var subscription = await _context.CustomerPackageSubscriptions
                .Include(s => s.PackageServiceUsages)
                .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);

            if (subscription == null || subscription.Status != SubscriptionStatusEnum.Active.ToString())
                return;

            var allFullyUsed = subscription.PackageServiceUsages
                .All(u => u.RemainingQuantity == 0);

            if (allFullyUsed)
            {
                subscription.Status = SubscriptionStatusEnum.FullyUsed.ToString();
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Subscription {SubscriptionId} marked as FullyUsed", subscriptionId);
            }
        }

        #endregion
    }
}


