using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.CustomerTypes.Entities; // ? Thêm namespace cho CustomerType
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    /// <summary>
    /// Seeder cho CustomerTypes - ph?c v? discount testing
    /// ?? Seeder này s? XÓA T?T C? CustomerTypes c? và t?o m?i
    /// </summary>
    public class CustomerTypeSeeder
    {
        public static async Task SeedAsync(EVDbContext context, ILogger logger)
        {
            try
            {
                // ========================================
                // STEP 1: XÓA T?T C? CUSTOMERTYPE C?
                // ========================================
                
                logger.LogInformation("??? Cleaning up old CustomerTypes...");

                // Xóa t?t c? CustomerTypes c?
                var oldCustomerTypes = await context.CustomerTypes.ToListAsync();
                
                if (oldCustomerTypes.Any())
                {
                    // Set TypeId = NULL cho t?t c? customers tr??c khi xóa (? Fixed: CustomerTypeId ? TypeId)
                    await context.Database.ExecuteSqlRawAsync(
                        "UPDATE Customers SET TypeID = NULL WHERE TypeID IS NOT NULL");

                    context.CustomerTypes.RemoveRange(oldCustomerTypes);
                    await context.SaveChangesAsync();

                    logger.LogInformation(
                        "? Deleted {Count} old CustomerTypes (Individual, Platinum, Corporate, Diamond)",
                        oldCustomerTypes.Count);
                }

                // ========================================
                // STEP 2: T?O CUSTOMERTYPE M?I
                // ========================================

                logger.LogInformation("?? Creating new CustomerTypes for discount testing...");

                var newCustomerTypes = new List<CustomerType>
                {
                    new CustomerType
                    {
                        TypeName = "Standard",
                        Description = "Khách hàng th??ng - Không có ?u ?ãi",
                        DiscountPercent = 0,
                        IsActive = true
                    },
                    new CustomerType
                    {
                        TypeName = "Silver",
                        Description = "Khách hàng b?c - Gi?m 5%",
                        DiscountPercent = 5,
                        IsActive = true
                    },
                    new CustomerType
                    {
                        TypeName = "Gold",
                        Description = "Khách hàng vàng - Gi?m 10%",
                        DiscountPercent = 10,
                        IsActive = true
                    },
                    new CustomerType
                    {
                        TypeName = "VIP",
                        Description = "Khách hàng VIP - Gi?m 15%",
                        DiscountPercent = 15,
                        IsActive = true
                    },
                    new CustomerType
                    {
                        TypeName = "Diamond",
                        Description = "Khách hàng kim c??ng - Gi?m 20%",
                        DiscountPercent = 20,
                        IsActive = true
                    }
                };

                await context.CustomerTypes.AddRangeAsync(newCustomerTypes);
                await context.SaveChangesAsync();

                logger.LogInformation(
                    "? Created {Count} new CustomerTypes: {Types}",
                    newCustomerTypes.Count,
                    string.Join(", ", newCustomerTypes.Select(ct => $"{ct.TypeName} ({ct.DiscountPercent}%)")));

                // ========================================
                // STEP 3: ASSIGN CUSTOMERTYPE CHO CUSTOMERS
                // ========================================

                logger.LogInformation("?? Assigning CustomerTypes to existing customers...");

                // L?y CustomerType IDs
                var standardType = await context.CustomerTypes.FirstAsync(ct => ct.TypeName == "Standard");
                var silverType = await context.CustomerTypes.FirstAsync(ct => ct.TypeName == "Silver");
                var goldType = await context.CustomerTypes.FirstAsync(ct => ct.TypeName == "Gold");
                var vipType = await context.CustomerTypes.FirstAsync(ct => ct.TypeName == "VIP");
                var diamondType = await context.CustomerTypes.FirstAsync(ct => ct.TypeName == "Diamond");

                // L?y t?t c? customers
                var customers = await context.Customers.ToListAsync();

                if (customers.Any())
                {
                    // Assign CustomerType theo pattern (?? test ?a d?ng)
                    for (int i = 0; i < customers.Count; i++)
                    {
                        var customer = customers[i];

                        // Pattern: 40% Standard, 25% Silver, 20% Gold, 10% VIP, 5% Diamond
                        var randomValue = i % 20;

                        if (randomValue < 8) // 40% (0-7)
                            customer.TypeId = standardType.TypeId; // ? Fixed: CustomerTypeId ? TypeId
                        else if (randomValue < 13) // 25% (8-12)
                            customer.TypeId = silverType.TypeId;
                        else if (randomValue < 17) // 20% (13-16)
                            customer.TypeId = goldType.TypeId;
                        else if (randomValue < 19) // 10% (17-18)
                            customer.TypeId = vipType.TypeId;
                        else // 5% (19)
                            customer.TypeId = diamondType.TypeId;
                    }

                    // ??m b?o có ít nh?t 1 customer m?i lo?i (?? test)
                    if (customers.Count >= 5)
                    {
                        customers[0].TypeId = standardType.TypeId; // ? Fixed: CustomerTypeId ? TypeId
                        customers[1].TypeId = silverType.TypeId;
                        customers[2].TypeId = goldType.TypeId;
                        customers[3].TypeId = vipType.TypeId;
                        customers[4].TypeId = diamondType.TypeId;
                    }

                    await context.SaveChangesAsync();

                    logger.LogInformation(
                        "? Assigned CustomerTypes to {Count} customers",
                        customers.Count);

                    // Log distribution
                    var distribution = customers
                        .GroupBy(c => c.TypeId) // ? Fixed: CustomerTypeId ? TypeId
                        .Select(g => new
                        {
                            TypeId = g.Key,
                            Count = g.Count()
                        })
                        .OrderBy(x => x.TypeId)
                        .ToList();

                    foreach (var dist in distribution)
                    {
                        var typeName = newCustomerTypes.First(ct => ct.TypeId == dist.TypeId).TypeName;
                        logger.LogInformation(
                            "  - {TypeName}: {Count} customers",
                            typeName, dist.Count);
                    }
                }

                logger.LogInformation("?? CustomerType seeder completed successfully!");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Error seeding CustomerTypes");
                throw;
            }
        }
    }
}
