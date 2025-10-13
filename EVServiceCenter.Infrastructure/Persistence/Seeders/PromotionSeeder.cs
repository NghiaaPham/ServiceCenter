using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    /// <summary>
    /// Seeder cho Promotions - t?o promotion codes ?? test discount
    /// </summary>
    public class PromotionSeeder
    {
        public static async Task SeedAsync(EVDbContext context, ILogger logger)
        {
            try
            {
                if (await context.Promotions.AnyAsync())
                {
                    logger.LogInformation("?? Promotions already seeded. Skipping...");
                    return;
                }

                logger.LogInformation("?? Creating promotions for discount testing...");

                var now = DateTime.UtcNow;

                var promotions = new List<Promotion>
                {
                    // Active promotions
                    new Promotion
                    {
                        PromotionCode = "SUMMER2024",
                        PromotionName = "Khuy?n mãi hè 2024",
                        PromotionType = "Percentage", // ? Fixed: DiscountType ? PromotionType
                        DiscountValue = 20, // ? Fixed: DiscountPercent ? DiscountValue
                        MinimumAmount = 100000, // ? Fixed: MinOrderAmount ? MinimumAmount
                        MaximumDiscount = 500000, // ? Fixed: MaxDiscountAmount ? MaximumDiscount
                        StartDate = DateOnly.FromDateTime(now.AddDays(-30)), // ? Fixed: DateTime ? DateOnly
                        EndDate = DateOnly.FromDateTime(now.AddDays(60)),
                        UsageLimit = 100,
                        UsageCount = 0, // ? Fixed: CurrentUsage ? UsageCount
                        IsActive = true,
                        Terms = "Áp d?ng cho t?t c? d?ch v?",
                        CreatedDate = now
                    },
                    new Promotion
                    {
                        PromotionCode = "NEWCUSTOMER10",
                        PromotionName = "?u ?ãi khách hàng m?i",
                        PromotionType = "Percentage",
                        DiscountValue = 10,
                        MinimumAmount = 0,
                        MaximumDiscount = 300000,
                        StartDate = DateOnly.FromDateTime(now.AddDays(-60)),
                        EndDate = DateOnly.FromDateTime(now.AddDays(120)),
                        UsageLimit = 500,
                        UsageCount = 0,
                        IsActive = true,
                        Terms = "Dành cho khách hàng m?i",
                        CreatedDate = now
                    },
                    new Promotion
                    {
                        PromotionCode = "FIXEDOFF50K",
                        PromotionName = "Gi?m c? ??nh 50k",
                        PromotionType = "FixedAmount",
                        DiscountValue = 50000,
                        MinimumAmount = 200000,
                        MaximumDiscount = null,
                        StartDate = DateOnly.FromDateTime(now.AddDays(-15)),
                        EndDate = DateOnly.FromDateTime(now.AddDays(45)),
                        UsageLimit = 200,
                        UsageCount = 0,
                        IsActive = true,
                        Terms = "Gi?m tr?c ti?p 50,000? cho ??n hàng t? 200,000?",
                        CreatedDate = now
                    },
                    new Promotion
                    {
                        PromotionCode = "VIP25",
                        PromotionName = "?u ?ãi VIP 25%",
                        PromotionType = "Percentage",
                        DiscountValue = 25,
                        MinimumAmount = 500000,
                        MaximumDiscount = 1000000,
                        StartDate = DateOnly.FromDateTime(now.AddDays(-10)),
                        EndDate = DateOnly.FromDateTime(now.AddDays(50)),
                        UsageLimit = 50,
                        UsageCount = 0,
                        IsActive = true,
                        CustomerTypes = "VIP,Diamond",
                        Terms = "Ch? dành cho khách hàng VIP và Diamond",
                        CreatedDate = now
                    },

                    // Expired promotion (for testing)
                    new Promotion
                    {
                        PromotionCode = "EXPIRED_CODE",
                        PromotionName = "Promotion ?ã h?t h?n",
                        PromotionType = "Percentage",
                        DiscountValue = 15,
                        MinimumAmount = 0,
                        MaximumDiscount = 200000,
                        StartDate = DateOnly.FromDateTime(now.AddDays(-90)),
                        EndDate = DateOnly.FromDateTime(now.AddDays(-30)), // Expired
                        UsageLimit = 50,
                        UsageCount = 0,
                        IsActive = false,
                        Terms = "Test expired promotion",
                        CreatedDate = now.AddDays(-90)
                    },

                    // Usage limit reached (for testing)
                    new Promotion
                    {
                        PromotionCode = "LIMITED_CODE",
                        PromotionName = "Promotion ?ã h?t l??t",
                        PromotionType = "Percentage",
                        DiscountValue = 25,
                        MinimumAmount = 0,
                        MaximumDiscount = 500000,
                        StartDate = DateOnly.FromDateTime(now.AddDays(-15)),
                        EndDate = DateOnly.FromDateTime(now.AddDays(30)),
                        UsageLimit = 10,
                        UsageCount = 10, // Already full
                        IsActive = true,
                        Terms = "Test usage limit",
                        CreatedDate = now.AddDays(-15)
                    }
                };

                await context.Promotions.AddRangeAsync(promotions);
                await context.SaveChangesAsync();

                logger.LogInformation(
                    "? Seeded {Count} promotions: {Codes}",
                    promotions.Count,
                    string.Join(", ", promotions.Select(p => p.PromotionCode)));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Error seeding Promotions");
                throw;
            }
        }
    }
}
