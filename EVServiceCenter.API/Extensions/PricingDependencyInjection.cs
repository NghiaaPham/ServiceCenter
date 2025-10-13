using EVServiceCenter.Core.Domains.Pricing.Interfaces;
using EVServiceCenter.Infrastructure.Domains.Pricing.Services;

namespace EVServiceCenter.API.Extensions
{
    /// <summary>
    /// Dependency Injection configuration for Pricing domain
    /// Includes discount calculation and promotion services
    /// </summary>
    public static class PricingDependencyInjection
    {
        public static IServiceCollection AddPricingModule(this IServiceCollection services)
        {
            // ═══════════════════════════════════════════════════════════
            // DISCOUNT CALCULATION SERVICES
            // ═══════════════════════════════════════════════════════════

            // Discount Calculator: 3-tier discount system
            // Tier 1: Subscription = 100% free (highest priority)
            // Tier 2: CustomerType vs Promotion = Choose MAX
            // Tier 3: Manual admin adjustments
            services.AddScoped<IDiscountCalculationService, DiscountCalculationService>();

            // Promotion Service: Validate and manage promotion codes
            services.AddScoped<IPromotionService, PromotionService>();

            return services;
        }
    }
}
