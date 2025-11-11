using EVServiceCenter.Core.Domains.CustomerTypes.Entities;
using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders;

public static class CustomerTypeSeeder
{
    /// <summary>
    /// ? IDEMPOTENT SEEDER: Only seed if CustomerTypes table is empty
    /// Prevents duplicate TypeID increment on every restart
    /// </summary>
    public static async Task SeedAsync(EVDbContext context, ILogger logger)
    {
        try
        {
            // ? CHECK: Skip if data already exists
            var existingCount = await context.CustomerTypes.CountAsync();
            
            if (existingCount > 0)
            {
                logger.LogInformation(
                    "? CustomerTypes already seeded ({Count} types exist). Skipping seeder to prevent duplicate TypeID increment.",
                    existingCount);
                return; // ? SKIP SEEDING
            }

            // ? ONLY SEED IF EMPTY
            logger.LogInformation("?? Seeding CustomerTypes (first time)...");

            var customerTypes = new List<CustomerType>
            {
                new CustomerType
                {
                    TypeName = "Standard",
                    DiscountPercent = 0,
                    Description = "Khách hàng th??ng - Không có gi?m giá",
                    IsActive = true
                },
                new CustomerType
                {
                    TypeName = "Silver",
                    DiscountPercent = 5,
                    Description = "Khách hàng b?c - Gi?m 5%",
                    IsActive = true
                },
                new CustomerType
                {
                    TypeName = "Gold",
                    DiscountPercent = 10,
                    Description = "Khách hàng vàng - Gi?m 10%",
                    IsActive = true
                },
                new CustomerType
                {
                    TypeName = "VIP",
                    DiscountPercent = 15,
                    Description = "Khách hàng VIP - Gi?m 15%",
                    IsActive = true
                },
                new CustomerType
                {
                    TypeName = "Diamond",
                    DiscountPercent = 20,
                    Description = "Khách hàng kim c??ng - Gi?m 20%",
                    IsActive = true
                }
            };

            await context.CustomerTypes.AddRangeAsync(customerTypes);
            await context.SaveChangesAsync();

            logger.LogInformation("? Seeded {Count} CustomerTypes successfully", customerTypes.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "? Error seeding CustomerTypes");
            throw;
        }
    }
}

