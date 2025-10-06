using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Repositories
{
    /// <summary>
    /// Query Repository Implementation cho Package Subscription
    /// Chứa tất cả READ operations - Không thay đổi data
    /// </summary>
    public class PackageSubscriptionQueryRepository : IPackageSubscriptionQueryRepository
    {
        private readonly EVDbContext _context;
        private readonly ILogger<PackageSubscriptionQueryRepository> _logger;

        public PackageSubscriptionQueryRepository(
            EVDbContext context,
            ILogger<PackageSubscriptionQueryRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region GetCustomerSubscriptionsAsync

        public async Task<List<PackageSubscriptionSummaryDto>> GetCustomerSubscriptionsAsync(
            int customerId,
            SubscriptionStatusEnum? statusFilter = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _context.CustomerPackageSubscriptions
                    .Where(s => s.CustomerId == customerId);

                // Filter by status if provided
                if (statusFilter.HasValue)
                {
                    var statusString = statusFilter.Value.ToString();
                    query = query.Where(s => s.Status == statusString);
                }

                var subscriptions = await query
                    .Include(s => s.Package)
                    .Include(s => s.Vehicle)
                        .ThenInclude(v => v.Model)
                    .Include(s => s.PackageServiceUsages)
                    .OrderByDescending(s => s.StartDate)
                    .ToListAsync(cancellationToken);

                return subscriptions.Select(MapToSummaryDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscriptions for customer {CustomerId}", customerId);
                throw;
            }
        }

        #endregion

        #region GetSubscriptionByIdAsync

        public async Task<PackageSubscriptionResponseDto?> GetSubscriptionByIdAsync(
            int subscriptionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var subscription = await _context.CustomerPackageSubscriptions
                    .Include(s => s.Package)
                    .Include(s => s.Customer)
                    .Include(s => s.Vehicle)
                        .ThenInclude(v => v.Model)
                    .Include(s => s.PackageServiceUsages)
                        .ThenInclude(u => u.Service)
                    .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);

                if (subscription == null)
                    return null;

                return MapToResponseDto(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting subscription by ID {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        #endregion

        #region GetActiveSubscriptionsForVehicleAsync

        public async Task<List<PackageSubscriptionSummaryDto>> GetActiveSubscriptionsForVehicleAsync(
            int vehicleId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var activeStatus = SubscriptionStatusEnum.Active.ToString();

                var subscriptions = await _context.CustomerPackageSubscriptions
                    .Where(s => s.VehicleId == vehicleId && s.Status == activeStatus)
                    .Include(s => s.Package)
                    .Include(s => s.Vehicle)
                        .ThenInclude(v => v.Model)
                    .Include(s => s.PackageServiceUsages)
                    .OrderByDescending(s => s.StartDate)
                    .ToListAsync(cancellationToken);

                return subscriptions.Select(MapToSummaryDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active subscriptions for vehicle {VehicleId}", vehicleId);
                throw;
            }
        }

        #endregion

        #region HasActiveSubscriptionForPackageAsync

        public async Task<bool> HasActiveSubscriptionForPackageAsync(
            int customerId,
            int vehicleId,
            int packageId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var activeStatus = SubscriptionStatusEnum.Active.ToString();

                return await _context.CustomerPackageSubscriptions
                    .AnyAsync(s =>
                        s.CustomerId == customerId &&
                        s.VehicleId == vehicleId &&
                        s.PackageId == packageId &&
                        s.Status == activeStatus,
                        cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking active subscription for customer {CustomerId}, vehicle {VehicleId}, package {PackageId}",
                    customerId, vehicleId, packageId);
                throw;
            }
        }

        #endregion

        #region GetSubscriptionUsageDetailsAsync

        public async Task<List<PackageServiceUsageDto>> GetSubscriptionUsageDetailsAsync(
            int subscriptionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var usages = await _context.PackageServiceUsages
                    .Where(u => u.SubscriptionId == subscriptionId)
                    .Include(u => u.Service)
                    .OrderBy(u => u.Service.ServiceName)
                    .ToListAsync(cancellationToken);

                return usages.Select(MapToUsageDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage details for subscription {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        #endregion

        #region HasRemainingUsageForServiceAsync

        public async Task<bool> HasRemainingUsageForServiceAsync(
            int subscriptionId,
            int serviceId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var usage = await _context.PackageServiceUsages
                    .FirstOrDefaultAsync(u =>
                        u.SubscriptionId == subscriptionId &&
                        u.ServiceId == serviceId,
                        cancellationToken);

                if (usage == null)
                    return false;

                return usage.RemainingQuantity > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking remaining usage for subscription {SubscriptionId}, service {ServiceId}",
                    subscriptionId, serviceId);
                throw;
            }
        }

        #endregion

        #region SubscriptionExistsAsync

        public async Task<bool> SubscriptionExistsAsync(
            int subscriptionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.CustomerPackageSubscriptions
                    .AnyAsync(s => s.SubscriptionId == subscriptionId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking subscription exists {SubscriptionId}", subscriptionId);
                throw;
            }
        }

        #endregion

        #region IsSubscriptionOwnedByCustomerAsync

        public async Task<bool> IsSubscriptionOwnedByCustomerAsync(
            int subscriptionId,
            int customerId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.CustomerPackageSubscriptions
                    .AnyAsync(s =>
                        s.SubscriptionId == subscriptionId &&
                        s.CustomerId == customerId,
                        cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error checking ownership for subscription {SubscriptionId}, customer {CustomerId}",
                    subscriptionId, customerId);
                throw;
            }
        }

        #endregion

        #region Private Mapping Methods

        /// <summary>
        /// Map entity to SummaryDto (for list view)
        /// </summary>
        private static PackageSubscriptionSummaryDto MapToSummaryDto(CustomerPackageSubscription subscription)
        {
            var usages = subscription.PackageServiceUsages?.ToList() ?? new List<PackageServiceUsage>();

            var totalAllowed = usages.Sum(u => u.TotalAllowedQuantity);
            var totalUsed = usages.Sum(u => u.UsedQuantity);
            var totalRemaining = usages.Sum(u => u.RemainingQuantity);

            var usagePercentage = totalAllowed > 0
                ? (decimal)totalUsed / totalAllowed * 100
                : 0;

            // Convert DateOnly? to DateTime? for DTO
            DateTime? expiryDate = subscription.ExpirationDate.HasValue
                ? subscription.ExpirationDate.Value.ToDateTime(TimeOnly.MinValue)
                : null;

            var daysUntilExpiry = subscription.ExpirationDate.HasValue
                ? (subscription.ExpirationDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow).Days
                : (int?)null;

            var canUse = subscription.Status == SubscriptionStatusEnum.Active.ToString()
                && (!subscription.ExpirationDate.HasValue || subscription.ExpirationDate.Value >= DateOnly.FromDateTime(DateTime.UtcNow))
                && totalRemaining > 0;

            string? warningMessage = null;
            if (daysUntilExpiry.HasValue && daysUntilExpiry.Value <= 7 && daysUntilExpiry.Value > 0)
            {
                warningMessage = $"Sắp hết hạn (còn {daysUntilExpiry.Value} ngày)";
            }
            else if (totalRemaining == 1)
            {
                warningMessage = "Chỉ còn 1 lượt";
            }
            else if (totalRemaining <= 3 && totalRemaining > 1)
            {
                warningMessage = $"Chỉ còn {totalRemaining} lượt";
            }

            return new PackageSubscriptionSummaryDto
            {
                SubscriptionId = subscription.SubscriptionId,
                PackageCode = subscription.Package?.PackageCode ?? "",
                PackageName = subscription.Package?.PackageName ?? "",
                PackageImageUrl = subscription.Package?.ImageUrl,
                VehiclePlateNumber = subscription.Vehicle?.LicensePlate ?? "",
                VehicleModelName = subscription.Vehicle?.Model?.ModelName ?? "",
                PurchaseDate = subscription.PurchaseDate ?? subscription.StartDate.ToDateTime(TimeOnly.MinValue),
                ExpiryDate = expiryDate,
                PricePaid = subscription.PaymentAmount ?? 0,
                Status = Enum.TryParse<SubscriptionStatusEnum>(subscription.Status, out var status)
                    ? status
                    : SubscriptionStatusEnum.Active,
                StatusDisplayName = subscription.Status switch
                {
                    "Active" => "Đang hoạt động",
                    "FullyUsed" => "Đã dùng hết",
                    "Expired" => "Đã hết hạn",
                    "Cancelled" => "Đã hủy",
                    "Suspended" => "Tạm dừng",
                    _ => subscription.Status ?? ""
                },
                TotalServicesCount = usages.Count,
                UsageStatus = $"{totalUsed}/{totalAllowed}",
                UsagePercentage = usagePercentage,
                DaysUntilExpiry = daysUntilExpiry > 0 ? daysUntilExpiry : null,
                CanUse = canUse,
                WarningMessage = warningMessage
            };
        }

        /// <summary>
        /// Map entity to ResponseDto (full details)
        /// </summary>
        private static PackageSubscriptionResponseDto MapToResponseDto(CustomerPackageSubscription subscription)
        {
            var usages = subscription.PackageServiceUsages?.ToList() ?? new List<PackageServiceUsage>();

            // Convert DateOnly to DateTime for DTOs
            DateTime purchaseDate = subscription.PurchaseDate ?? subscription.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime startDate = subscription.StartDate.ToDateTime(TimeOnly.MinValue);
            DateTime? expiryDate = subscription.ExpirationDate.HasValue
                ? subscription.ExpirationDate.Value.ToDateTime(TimeOnly.MinValue)
                : null;

            return new PackageSubscriptionResponseDto
            {
                SubscriptionId = subscription.SubscriptionId,
                CustomerId = subscription.CustomerId,
                CustomerName = subscription.Customer?.FullName ?? "",
                VehicleId = subscription.VehicleId ?? 0,
                VehiclePlateNumber = subscription.Vehicle?.LicensePlate ?? "",
                VehicleModelName = subscription.Vehicle?.Model?.ModelName ?? "",
                PackageId = subscription.PackageId,
                PackageCode = subscription.Package?.PackageCode ?? "",
                PackageName = subscription.Package?.PackageName ?? "",
                PackageDescription = subscription.Package?.Description,
                PackageImageUrl = subscription.Package?.ImageUrl,
                PurchaseDate = purchaseDate,
                StartDate = startDate,
                ExpiryDate = expiryDate,
                ValidityPeriodInDays = subscription.Package?.ValidityPeriod,
                ValidityMileage = subscription.Package?.ValidityMileage,
                InitialVehicleMileage = subscription.InitialVehicleMileage,
                PricePaid = subscription.PaymentAmount ?? 0,
                Status = Enum.TryParse<SubscriptionStatusEnum>(subscription.Status, out var status)
                    ? status
                    : SubscriptionStatusEnum.Active,
                StatusDisplayName = subscription.Status switch
                {
                    "Active" => "Đang hoạt động",
                    "FullyUsed" => "Đã dùng hết",
                    "Expired" => "Đã hết hạn",
                    "Cancelled" => "Đã hủy",
                    "Suspended" => "Tạm dừng",
                    _ => subscription.Status ?? ""
                },
                CancellationReason = subscription.CancellationReason,
                CancelledDate = subscription.CancelledDate.HasValue
                    ? subscription.CancelledDate.Value.ToDateTime(TimeOnly.MinValue)
                    : null,
                CustomerNotes = subscription.Notes,
                ServiceUsages = usages.Select(MapToUsageDto).ToList()
            };
        }

        /// <summary>
        /// Map PackageServiceUsage entity to DTO
        /// </summary>
        private static PackageServiceUsageDto MapToUsageDto(PackageServiceUsage usage)
        {
            return new PackageServiceUsageDto
            {
                UsageId = usage.UsageId,
                ServiceId = usage.ServiceId,
                ServiceName = usage.Service?.ServiceName ?? "",
                ServiceDescription = usage.Service?.Description,
                TotalAllowedQuantity = usage.TotalAllowedQuantity,
                UsedQuantity = usage.UsedQuantity,
                RemainingQuantity = usage.RemainingQuantity,
                LastUsedDate = usage.LastUsedDate,
                LastUsedAppointmentId = usage.LastUsedAppointmentId
            };
        }

        #endregion
    }
}
