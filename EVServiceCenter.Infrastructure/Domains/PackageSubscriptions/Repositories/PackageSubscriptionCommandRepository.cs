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
            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
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
                    Status = SubscriptionStatusEnum.Active.ToString(),
                    Notes = string.IsNullOrWhiteSpace(request.CustomerNotes)
                        ? null
                        : request.CustomerNotes.Trim()
                };

                _context.CustomerPackageSubscriptions.Add(subscription);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Created subscription {SubscriptionId} for customer {CustomerId}",
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

                _logger.LogInformation("Successfully purchased subscription {SubscriptionId} - {PackageName}",
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
                    "🔒 [PESSIMISTIC LOCK] Updating usage: SubscriptionId={SubscriptionId}, " +
                    "ServiceId={ServiceId}, QuantityToDeduct={Quantity}, AppointmentId={AppointmentId}",
                    subscriptionId, serviceId, quantityUsed, appointmentId);

                // 🔒 BƯỚC 1: SELECT với UPDLOCK, ROWLOCK để lock row
                // UPDLOCK: Shared lock cho read, nhưng signal rằng sẽ update sau
                // ROWLOCK: Chỉ lock row này (không lock toàn table/page)
                // WITH (UPDLOCK, ROWLOCK) ngăn dirty reads và lost updates
                var sql = @"
                    SELECT UsageID, SubscriptionID, ServiceID,
                           TotalAllowedQuantity, UsedQuantity, RemainingQuantity,
                           LastUsedDate, LastUsedAppointmentID
                    FROM PackageServiceUsages WITH (UPDLOCK, ROWLOCK)
                    WHERE SubscriptionID = {0} AND ServiceID = {1}";

                var usageList = await _context.PackageServiceUsages
                    .FromSqlRaw(sql, subscriptionId, serviceId)
                    .ToListAsync(cancellationToken);

                var usage = usageList.FirstOrDefault();

                if (usage == null)
                {
                    _logger.LogWarning(
                        "⚠️ Service usage NOT FOUND: SubscriptionId={SubscriptionId}, ServiceId={ServiceId} " +
                        "→ Return FALSE",
                        subscriptionId, serviceId);
                    return false;
                }

                // 🔒 BƯỚC 2: CHECK RemainingQuantity TRONG LOCK
                // Tại thời điểm này, row đã bị lock, không ai khác có thể đọc/ghi
                var remainingQuantityTruocKhiTru = usage.RemainingQuantity;

                _logger.LogInformation(
                    "🔍 Locked row: UsageId={UsageId}, RemainingQuantity={Remaining}, " +
                    "QuantityToDeduct={ToDeduct}",
                    usage.UsageId, remainingQuantityTruocKhiTru, quantityUsed);

                // Nếu KHÔNG ĐỦ LƯỢT → Return FALSE (KHÔNG throw exception)
                // Caller sẽ handle graceful degradation
                if (remainingQuantityTruocKhiTru < quantityUsed)
                {
                    _logger.LogWarning(
                        "⚠️ INSUFFICIENT USAGE: SubscriptionId={SubscriptionId}, ServiceId={ServiceId}, " +
                        "Remaining={Remaining}, Required={Required} → Return FALSE (graceful degradation)",
                        subscriptionId, serviceId, remainingQuantityTruocKhiTru, quantityUsed);

                    return false; // ❌ KHÔNG ĐỦ LƯỢT - Graceful degradation
                }

                // 🔒 BƯỚC 3: UPDATE USAGE (vẫn trong lock)
                var usedQuantityTruocKhiTru = usage.UsedQuantity;
                var usedQuantitySauKhiTru = usedQuantityTruocKhiTru + quantityUsed;
                var remainingQuantitySauKhiTru = remainingQuantityTruocKhiTru - quantityUsed;

                usage.UsedQuantity = usedQuantitySauKhiTru;
                usage.RemainingQuantity = remainingQuantitySauKhiTru;
                usage.LastUsedDate = DateTime.UtcNow;
                usage.LastUsedAppointmentId = appointmentId;

                _logger.LogInformation(
                    "✏️ Updating usage: UsageId={UsageId}, " +
                    "UsedQuantity: {OldUsed} → {NewUsed}, " +
                    "RemainingQuantity: {OldRemaining} → {NewRemaining}",
                    usage.UsageId,
                    usedQuantityTruocKhiTru, usedQuantitySauKhiTru,
                    remainingQuantityTruocKhiTru, remainingQuantitySauKhiTru);

                // 🔒 BƯỚC 4: SAVE CHANGES (commit update và release lock)
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "✅ Usage deducted successfully: SubscriptionId={SubscriptionId}, " +
                    "ServiceId={ServiceId}, RemainingQuantity={NewRemaining}",
                    subscriptionId, serviceId, remainingQuantitySauKhiTru);

                // 🔒 BƯỚC 5: CHECK IF SUBSCRIPTION FULLY USED
                // (Chạy sau SaveChanges, không cần lock nữa)
                await CheckAndUpdateFullyUsedStatusAsync(subscriptionId, cancellationToken);

                return true; // ✅ TRỪ LƯỢT THÀNH CÔNG
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ ERROR updating service usage (with pessimistic lock): " +
                    "SubscriptionId={SubscriptionId}, ServiceId={ServiceId}",
                    subscriptionId, serviceId);
                throw;
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

                if (subscription.Status != SubscriptionStatusEnum.Suspended.ToString())
                {
                    throw new InvalidOperationException("Chỉ có thể kích hoạt lại subscription đang bị Suspended");
                }

                subscription.Status = SubscriptionStatusEnum.Active.ToString();
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
