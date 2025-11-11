using EVServiceCenter.Core.Domains.Pricing.Interfaces;
using EVServiceCenter.Core.Domains.Pricing.Models;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace EVServiceCenter.Infrastructure.Domains.Pricing.Services
{
    /// <summary>
    /// Service quản lý và validate promotion codes
    /// </summary>
    public class PromotionService : IPromotionService
    {
        private readonly EVDbContext _context;
        private readonly ILogger<PromotionService> _logger;

        public PromotionService(
            EVDbContext context,
            ILogger<PromotionService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PromotionValidationResult> ValidatePromotionAsync(
            string promotionCode,
            int customerId,
            decimal orderAmount,
            List<int> serviceIds)
        {
            _logger.LogInformation(
                "Validating promotion code '{Code}' for customer {CustomerId}, orderAmount={Amount:N0}đ",
                promotionCode, customerId, orderAmount);

            // 1. Get promotion by code
            var promotion = await _context.Promotions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PromotionCode == promotionCode);

            if (promotion == null)
            {
                return PromotionValidationResult.Invalid("Mã khuyến mãi không tồn tại");
            }

            // 2. Check active
            if (!promotion.IsActive.GetValueOrDefault())
            {
                return PromotionValidationResult.Invalid("Mã khuyến mãi đã bị vô hiệu hóa");
            }

            // 3. Check date range
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (today < promotion.StartDate)
            {
                return PromotionValidationResult.Invalid(
                    $"Mã khuyến mãi chưa có hiệu lực (bắt đầu từ {promotion.StartDate:dd/MM/yyyy})");
            }

            if (today > promotion.EndDate)
            {
                return PromotionValidationResult.Invalid(
                    $"Mã khuyến mãi đã hết hạn (hết hạn vào {promotion.EndDate:dd/MM/yyyy})");
            }

            // 4. Check usage limit
            if (promotion.UsageLimit.HasValue &&
                promotion.UsageCount >= promotion.UsageLimit)
            {
                return PromotionValidationResult.Invalid(
                    "Mã khuyến mãi đã hết lượt sử dụng");
            }

            // 5. Check minimum amount
            if (promotion.MinimumAmount.HasValue &&
                orderAmount < promotion.MinimumAmount)
            {
                return PromotionValidationResult.Invalid(
                    $"Đơn hàng tối thiểu {promotion.MinimumAmount:N0}đ để sử dụng mã này");
            }

            // 6. Check applicable services
            if (!string.IsNullOrEmpty(promotion.ApplicableServices))
            {
                try
                {
                    var allowedServiceIds = JsonSerializer.Deserialize<List<int>>(
                        promotion.ApplicableServices);

                    if (allowedServiceIds != null && allowedServiceIds.Any())
                    {
                        // Check if at least one service in order is in allowed list
                        if (!serviceIds.Any(s => allowedServiceIds.Contains(s)))
                        {
                            return PromotionValidationResult.Invalid(
                                "Mã khuyến mãi không áp dụng cho các dịch vụ đã chọn");
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to parse ApplicableServices JSON for promotion {PromotionId}",
                        promotion.PromotionId);
                }
            }

            // 7. Check customer types
            if (!string.IsNullOrEmpty(promotion.CustomerTypes))
            {
                try
                {
                    var customer = await _context.Customers
                        .Include(c => c.Type)
                        .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                    if (customer?.Type != null)
                    {
                        var allowedTypes = JsonSerializer.Deserialize<List<string>>(
                            promotion.CustomerTypes);

                        if (allowedTypes != null && allowedTypes.Any())
                        {
                            if (!allowedTypes.Contains(customer.Type.TypeName))
                            {
                                return PromotionValidationResult.Invalid(
                                    $"Mã khuyến mãi chỉ dành cho khách hàng {string.Join(", ", allowedTypes)}");
                            }
                        }
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex,
                        "Failed to parse CustomerTypes JSON for promotion {PromotionId}",
                        promotion.PromotionId);
                }
            }

            // 8. Calculate discount
            decimal discountAmount = 0;

            if (promotion.PromotionType == "Percentage")
            {
                if (!promotion.DiscountValue.HasValue)
                {
                    return PromotionValidationResult.Invalid("Cấu hình mã khuyến mãi không hợp lệ");
                }

                discountAmount = orderAmount * (promotion.DiscountValue.Value / 100);

                // Cap at maximum discount
                if (promotion.MaximumDiscount.HasValue &&
                    discountAmount > promotion.MaximumDiscount.Value)
                {
                    discountAmount = promotion.MaximumDiscount.Value;

                    _logger.LogInformation(
                        "Promotion discount capped at maximum: {MaxDiscount:N0}đ",
                        promotion.MaximumDiscount.Value);
                }

                _logger.LogInformation(
                    "Calculated percentage discount: {Percent}% of {OrderAmount:N0}đ = {Discount:N0}đ",
                    promotion.DiscountValue.Value, orderAmount, discountAmount);
            }
            else if (promotion.PromotionType == "FixedAmount")
            {
                if (!promotion.DiscountValue.HasValue)
                {
                    return PromotionValidationResult.Invalid("Cấu hình mã khuyến mãi không hợp lệ");
                }

                discountAmount = promotion.DiscountValue.Value;

                _logger.LogInformation(
                    "Fixed amount discount: {Discount:N0}đ",
                    discountAmount);
            }
            else
            {
                return PromotionValidationResult.Invalid(
                    $"Loại khuyến mãi '{promotion.PromotionType}' không được hỗ trợ");
            }

            // Ensure discount doesn't exceed order amount
            if (discountAmount > orderAmount)
            {
                discountAmount = orderAmount;
                _logger.LogWarning(
                    "Discount amount ({Discount:N0}đ) exceeds order amount ({OrderAmount:N0}đ), capping to order amount",
                    discountAmount, orderAmount);
            }

            _logger.LogInformation(
                "✅ Promotion '{Code}' validated successfully: {Discount:N0}đ discount",
                promotionCode, discountAmount);

            return PromotionValidationResult.Valid(promotion.PromotionId, discountAmount);
        }

        public async Task IncrementUsageAsync(string promotionCode)
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.PromotionCode == promotionCode);

            if (promotion != null)
            {
                promotion.UsageCount = (promotion.UsageCount ?? 0) + 1;
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Incremented usage count for promotion '{Code}': {Count}/{Limit}",
                    promotionCode, promotion.UsageCount, promotion.UsageLimit ?? -1);
            }
            else
            {
                _logger.LogWarning(
                    "Attempted to increment usage for non-existent promotion '{Code}'",
                    promotionCode);
            }
        }

        /// <summary>
        /// ✅ FIX GAP #8: Decrement promotion usage on cancellation
        /// </summary>
        public async Task DecrementUsageAsync(string promotionCode)
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.PromotionCode == promotionCode);

            if (promotion != null)
            {
                if (promotion.UsageCount > 0)
                {
                    var oldCount = promotion.UsageCount;
                    promotion.UsageCount = oldCount - 1;
                    await _context.SaveChangesAsync();

                    _logger.LogInformation(
                        "✅ GAP #8 - Decremented usage count for promotion '{Code}': {Old} → {New} (/{Limit})",
                        promotionCode, oldCount, promotion.UsageCount, promotion.UsageLimit ?? -1);
                }
                else
                {
                    _logger.LogWarning(
                        "⚠️ Cannot decrement usage for promotion '{Code}' - already at 0",
                        promotionCode);
                }
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ Attempted to decrement usage for non-existent promotion '{Code}'",
                    promotionCode);
            }
        }
    }
}
