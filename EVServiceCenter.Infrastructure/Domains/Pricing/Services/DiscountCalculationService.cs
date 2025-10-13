using EVServiceCenter.Core.Domains.Pricing.Interfaces;
using EVServiceCenter.Core.Domains.Pricing.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EVServiceCenter.Infrastructure.Domains.Pricing.Services
{
    /// <summary>
    /// Service tính toán discount theo quy tắc 3-Tier:
    /// TIER 1: Subscription services = 100% free (không apply discount khác)
    /// TIER 2: Regular/Extra services = MAX(CustomerType, Promotion)
    /// TIER 3: Manual admin discount (invoice level)
    /// </summary>
    public class DiscountCalculationService : IDiscountCalculationService
    {
        private readonly IPromotionService _promotionService;
        private readonly ILogger<DiscountCalculationService> _logger;

        public DiscountCalculationService(
            IPromotionService promotionService,
            ILogger<DiscountCalculationService> logger)
        {
            _promotionService = promotionService ?? throw new ArgumentNullException(nameof(promotionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DiscountCalculationResult> CalculateDiscountAsync(
            DiscountCalculationRequest request)
        {
            var result = new DiscountCalculationResult
            {
                ServiceBreakdowns = new List<ServiceDiscountBreakdown>()
            };

            _logger.LogInformation(
                "Starting discount calculation for customer {CustomerId}, {ServiceCount} services",
                request.CustomerId, request.Services.Count);

            // ═══════════════════════════════════════════════════════════
            // STEP 1: Tính Original Total (chỉ tính services có price > 0)
            // ═══════════════════════════════════════════════════════════

            decimal originalTotal = 0;
            var discountableServices = new List<ServiceLineItem>();

            foreach (var service in request.Services)
            {
                var originalPrice = service.BasePrice * service.Quantity;

                // ✅ RULE 1: Subscription services = 100% free, không cần discount
                if (service.ServiceSource == "Subscription")
                {
                    result.ServiceBreakdowns.Add(new ServiceDiscountBreakdown
                    {
                        ServiceId = service.ServiceId,
                        ServiceName = service.ServiceName,
                        ServiceSource = "Subscription",
                        OriginalPrice = originalPrice,
                        DiscountAmount = originalPrice, // 100% discount
                        FinalPrice = 0,
                        DiscountReason = "Dịch vụ từ gói subscription (miễn phí 100%)"
                    });

                    _logger.LogInformation(
                        "Service '{ServiceName}' is from subscription (subscriptionId={SubscriptionId}), price = 0",
                        service.ServiceName, service.SubscriptionId);

                    continue; // ← Bỏ qua, không tính vào originalTotal
                }

                // ✅ Services Regular/Extra mới tính vào originalTotal
                originalTotal += originalPrice;
                discountableServices.Add(service);

                _logger.LogDebug(
                    "Service '{ServiceName}' ({ServiceSource}): {Price}đ × {Qty} = {Total}đ",
                    service.ServiceName, service.ServiceSource, service.BasePrice,
                    service.Quantity, originalPrice);
            }

            result.OriginalTotal = originalTotal;

            _logger.LogInformation(
                "Original total (excluding subscription services): {OriginalTotal:N0}đ",
                originalTotal);

            // ═══════════════════════════════════════════════════════════
            // STEP 2: Tính CustomerType Discount (nếu có)
            // ═══════════════════════════════════════════════════════════

            decimal customerTypeDiscount = 0;

            if (request.CustomerTypeId.HasValue &&
                request.CustomerTypeDiscountPercent > 0)
            {
                customerTypeDiscount = originalTotal *
                    (request.CustomerTypeDiscountPercent.Value / 100);

                _logger.LogInformation(
                    "CustomerType discount: {Percent}% of {OriginalTotal:N0}đ = {Discount:N0}đ",
                    request.CustomerTypeDiscountPercent, originalTotal, customerTypeDiscount);
            }
            else
            {
                _logger.LogInformation("No CustomerType discount (customer type not set or discount = 0)");
            }

            result.CustomerTypeDiscount = customerTypeDiscount;

            // ═══════════════════════════════════════════════════════════
            // STEP 3: Tính Promotion Discount (nếu có)
            // ═══════════════════════════════════════════════════════════

            decimal promotionDiscount = 0;

            if (!string.IsNullOrEmpty(request.PromotionCode))
            {
                _logger.LogInformation("Validating promotion code: {Code}", request.PromotionCode);

                // Validate promotion
                var promotionValidation = await _promotionService.ValidatePromotionAsync(
                    promotionCode: request.PromotionCode,
                    customerId: request.CustomerId,
                    orderAmount: originalTotal,
                    serviceIds: discountableServices.Select(s => s.ServiceId).ToList());

                if (promotionValidation.IsValid)
                {
                    promotionDiscount = promotionValidation.DiscountAmount;
                    result.PromotionCodeUsed = request.PromotionCode;
                    result.PromotionId = promotionValidation.PromotionId;

                    _logger.LogInformation(
                        "Promotion '{Code}' validated successfully: {Discount:N0}đ discount",
                        request.PromotionCode, promotionDiscount);
                }
                else
                {
                    _logger.LogWarning(
                        "Promotion '{Code}' validation failed: {Reason}",
                        request.PromotionCode, promotionValidation.ErrorMessage);

                    // ❌ Promotion không hợp lệ → Không apply, không throw error
                    // Customer vẫn được CustomerType discount
                    promotionDiscount = 0;
                }
            }
            else
            {
                _logger.LogInformation("No promotion code provided");
            }

            result.PromotionDiscount = promotionDiscount;

            // ═══════════════════════════════════════════════════════════
            // STEP 4: ✅ RULE 3 - Choose MAX(CustomerType, Promotion)
            // ═══════════════════════════════════════════════════════════

            decimal finalDiscount = 0;
            string appliedDiscountType = "None";

            if (promotionDiscount > customerTypeDiscount)
            {
                finalDiscount = promotionDiscount;
                appliedDiscountType = "Promotion";

                _logger.LogInformation(
                    "✅ Applied PROMOTION discount: {PromotionDiscount:N0}đ > {CustomerTypeDiscount:N0}đ",
                    promotionDiscount, customerTypeDiscount);
            }
            else if (customerTypeDiscount > 0)
            {
                finalDiscount = customerTypeDiscount;
                appliedDiscountType = "CustomerType";

                _logger.LogInformation(
                    "✅ Applied CUSTOMER TYPE discount: {CustomerTypeDiscount:N0}đ >= {PromotionDiscount:N0}đ",
                    customerTypeDiscount, promotionDiscount);
            }
            else
            {
                finalDiscount = 0;
                appliedDiscountType = "None";

                _logger.LogInformation("No discount applied");
            }

            result.FinalDiscount = finalDiscount;
            result.AppliedDiscountType = appliedDiscountType;

            // ═══════════════════════════════════════════════════════════
            // STEP 5: Tính Final Total
            // ═══════════════════════════════════════════════════════════

            result.FinalTotal = originalTotal - finalDiscount;

            _logger.LogInformation(
                "📊 Final calculation: {OriginalTotal:N0}đ - {FinalDiscount:N0}đ = {FinalTotal:N0}đ",
                originalTotal, finalDiscount, result.FinalTotal);

            // ═══════════════════════════════════════════════════════════
            // STEP 6: Breakdown chi tiết từng service
            // ═══════════════════════════════════════════════════════════

            foreach (var service in discountableServices)
            {
                var serviceOriginalPrice = service.BasePrice * service.Quantity;

                // Tính discount cho service này (theo tỷ lệ)
                decimal serviceDiscount = 0;
                if (originalTotal > 0)
                {
                    serviceDiscount = (serviceOriginalPrice / originalTotal) * finalDiscount;
                }

                string discountReason = appliedDiscountType == "Promotion"
                    ? $"Mã khuyến mãi '{result.PromotionCodeUsed}'"
                    : appliedDiscountType == "CustomerType"
                    ? $"Giảm giá {request.CustomerTypeDiscountPercent}% cho khách hàng hạng cao"
                    : "Không có giảm giá";

                result.ServiceBreakdowns.Add(new ServiceDiscountBreakdown
                {
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    ServiceSource = service.ServiceSource,
                    OriginalPrice = serviceOriginalPrice,
                    DiscountAmount = serviceDiscount,
                    FinalPrice = serviceOriginalPrice - serviceDiscount,
                    DiscountReason = discountReason
                });
            }

            _logger.LogInformation(
                "✅ Discount calculation completed: {BreakdownCount} service breakdowns created",
                result.ServiceBreakdowns.Count);

            return result;
        }
    }
}
