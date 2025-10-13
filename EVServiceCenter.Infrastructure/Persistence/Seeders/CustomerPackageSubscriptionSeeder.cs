using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    /// <summary>
    /// Seeder for CustomerPackageSubscription and PackageServiceUsage
    /// Creates test subscriptions for customers to use when booking appointments
    /// </summary>
    public class CustomerPackageSubscriptionSeeder
    {
        private readonly EVDbContext _context;
        private readonly ILogger<CustomerPackageSubscriptionSeeder> _logger;

        public CustomerPackageSubscriptionSeeder(
            EVDbContext context,
            ILogger<CustomerPackageSubscriptionSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Seed CustomerPackageSubscriptions and their PackageServiceUsage records
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting CustomerPackageSubscription seeding...");

            // Check if subscriptions already exist
            var existingSubscriptions = await _context.CustomerPackageSubscriptions
                .Where(s => s.SubscriptionCode.StartsWith("SUB-"))
                .ToListAsync();

            if (existingSubscriptions.Any())
            {
                _logger.LogInformation("CustomerPackageSubscriptions already seeded. Skipping...");
                return;
            }

            // Get required data
            var customers = await GetCustomersAsync();
            var packages = await GetPackagesAsync();
            var vehicles = await GetVehiclesAsync();

            if (customers.Count == 0 || packages.Count == 0)
            {
                _logger.LogWarning("Not enough data. Please seed Customers and MaintenancePackages first.");
                return;
            }

            // Create subscriptions
            var subscriptions = CreateSubscriptions(customers, packages, vehicles);

            // Add subscriptions to context
            await _context.CustomerPackageSubscriptions.AddRangeAsync(subscriptions);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created {Count} customer subscriptions", subscriptions.Count);

            // Create PackageServiceUsage for each subscription
            var usageRecords = await CreatePackageServiceUsagesAsync(subscriptions);
            await _context.PackageServiceUsages.AddRangeAsync(usageRecords);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created {Count} package service usage records", usageRecords.Count);
            _logger.LogInformation("CustomerPackageSubscription seeding completed successfully!");
        }

        private async Task<List<int>> GetCustomersAsync()
        {
            return await _context.Customers
                .Where(c => c.IsActive == true)
                .Select(c => c.CustomerId)
                .Take(10)
                .ToListAsync();
        }

        private async Task<Dictionary<string, int>> GetPackagesAsync()
        {
            return await _context.MaintenancePackages
                .Where(p => p.IsActive == true)
                .ToDictionaryAsync(p => p.PackageCode, p => p.PackageId);
        }

        private async Task<Dictionary<int, int>> GetVehiclesAsync()
        {
            // Get vehicles mapping: CustomerId -> VehicleId
            return await _context.CustomerVehicles
                .GroupBy(v => v.CustomerId)
                .Select(g => new { CustomerId = g.Key, VehicleId = g.First().VehicleId })
                .ToDictionaryAsync(x => x.CustomerId, x => x.VehicleId);
        }

        private List<CustomerPackageSubscription> CreateSubscriptions(
            List<int> customerIds,
            Dictionary<string, int> packages,
            Dictionary<int, int> vehicles)
        {
            var subscriptions = new List<CustomerPackageSubscription>();
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Subscription 1: Customer 1 - Basic Package - Active
            if (customerIds.Count > 0 && packages.ContainsKey("PKG-BASIC-2025"))
            {
                var customerId = customerIds[0];
                subscriptions.Add(new CustomerPackageSubscription
                {
                    SubscriptionCode = "SUB-2025-001",
                    CustomerId = customerId,
                    PackageId = packages["PKG-BASIC-2025"],
                    VehicleId = vehicles.GetValueOrDefault(customerId),
                    StartDate = today.AddDays(-30),
                    ExpirationDate = today.AddDays(335), // ~11 months left
                    Status = "Active",
                    AutoRenew = false,
                    OriginalPrice = 2000000,
                    DiscountPercent = 20,
                    DiscountAmount = 400000,
                    PaymentAmount = 1600000,
                    PurchaseDate = DateTime.UtcNow.AddDays(-30),
                    InitialVehicleMileage = 5000,
                    RemainingServices = 8,  // 4 services x 2 times = 8
                    UsedServices = 0,
                    CreatedDate = DateTime.UtcNow.AddDays(-30),
                    Notes = "Test subscription for Basic package - Active"
                });
            }

            // Subscription 2: Customer 2 - Premium Package - Active (some services used)
            if (customerIds.Count > 1 && packages.ContainsKey("PKG-PREMIUM-2025"))
            {
                var customerId = customerIds[1];
                subscriptions.Add(new CustomerPackageSubscription
                {
                    SubscriptionCode = "SUB-2025-002",
                    CustomerId = customerId,
                    PackageId = packages["PKG-PREMIUM-2025"],
                    VehicleId = vehicles.GetValueOrDefault(customerId),
                    StartDate = today.AddDays(-90),
                    ExpirationDate = today.AddDays(275), // ~9 months left
                    Status = "Active",
                    AutoRenew = true,
                    OriginalPrice = 4500000,
                    DiscountPercent = 25,
                    DiscountAmount = 1125000,
                    PaymentAmount = 3375000,
                    PurchaseDate = DateTime.UtcNow.AddDays(-90),
                    InitialVehicleMileage = 8000,
                    RemainingServices = 14,  // 16 total - 2 used
                    UsedServices = 2,
                    LastServiceDate = today.AddDays(-15),
                    CreatedDate = DateTime.UtcNow.AddDays(-90),
                    Notes = "Test subscription for Premium package - Partially used"
                });
            }

            // Subscription 3: Customer 3 - VIP Package - Active (for testing)
            if (customerIds.Count > 2 && packages.ContainsKey("PKG-VIP-2025"))
            {
                var customerId = customerIds[2];
                subscriptions.Add(new CustomerPackageSubscription
                {
                    SubscriptionCode = "SUB-2025-003",
                    CustomerId = customerId,
                    PackageId = packages["PKG-VIP-2025"],
                    VehicleId = vehicles.GetValueOrDefault(customerId),
                    StartDate = today.AddDays(-60),
                    ExpirationDate = today.AddDays(670), // ~22 months left (2 years package)
                    Status = "Active",
                    AutoRenew = true,
                    OriginalPrice = 8000000,
                    DiscountPercent = 30,
                    DiscountAmount = 2400000,
                    PaymentAmount = 5600000,
                    PurchaseDate = DateTime.UtcNow.AddDays(-60),
                    InitialVehicleMileage = 12000,
                    RemainingServices = 131,  // VIP has many services (99 car wash)
                    UsedServices = 1,
                    LastServiceDate = today.AddDays(-20),
                    CreatedDate = DateTime.UtcNow.AddDays(-60),
                    Notes = "Test subscription for VIP package - Full benefits"
                });
            }

            // Subscription 4: Customer 4 - Premium Package - Expired
            if (customerIds.Count > 3 && packages.ContainsKey("PKG-PREMIUM-2025"))
            {
                var customerId = customerIds[3];
                subscriptions.Add(new CustomerPackageSubscription
                {
                    SubscriptionCode = "SUB-2024-004",
                    CustomerId = customerId,
                    PackageId = packages["PKG-PREMIUM-2025"],
                    VehicleId = vehicles.GetValueOrDefault(customerId),
                    StartDate = today.AddDays(-400),
                    ExpirationDate = today.AddDays(-35), // Expired
                    Status = "Expired",
                    AutoRenew = false,
                    OriginalPrice = 4500000,
                    DiscountPercent = 25,
                    DiscountAmount = 1125000,
                    PaymentAmount = 3375000,
                    PurchaseDate = DateTime.UtcNow.AddDays(-400),
                    InitialVehicleMileage = 3000,
                    RemainingServices = 0,
                    UsedServices = 16,
                    LastServiceDate = today.AddDays(-50),
                    CreatedDate = DateTime.UtcNow.AddDays(-400),
                    Notes = "Test subscription - Expired"
                });
            }

            // Subscription 5: Customer 5 - Basic Package - Active (new)
            if (customerIds.Count > 4 && packages.ContainsKey("PKG-BASIC-2025"))
            {
                var customerId = customerIds[4];
                subscriptions.Add(new CustomerPackageSubscription
                {
                    SubscriptionCode = "SUB-2025-005",
                    CustomerId = customerId,
                    PackageId = packages["PKG-BASIC-2025"],
                    VehicleId = vehicles.GetValueOrDefault(customerId),
                    StartDate = today.AddDays(-5),
                    ExpirationDate = today.AddDays(360),
                    Status = "Active",
                    AutoRenew = false,
                    OriginalPrice = 2000000,
                    DiscountPercent = 20,
                    DiscountAmount = 400000,
                    PaymentAmount = 1600000,
                    PurchaseDate = DateTime.UtcNow.AddDays(-5),
                    InitialVehicleMileage = 15000,
                    RemainingServices = 8,
                    UsedServices = 0,
                    CreatedDate = DateTime.UtcNow.AddDays(-5),
                    Notes = "Test subscription - Newly purchased"
                });
            }

            return subscriptions;
        }

        private async Task<List<PackageServiceUsage>> CreatePackageServiceUsagesAsync(
            List<CustomerPackageSubscription> subscriptions)
        {
            var usageRecords = new List<PackageServiceUsage>();

            foreach (var subscription in subscriptions)
            {
                // Get package services for this subscription
                var packageServices = await _context.PackageServices
                    .Include(ps => ps.Service)
                    .Where(ps => ps.PackageId == subscription.PackageId && ps.IncludedInPackage == true)
                    .ToListAsync();

                foreach (var packageService in packageServices)
                {
                    var totalQuantity = packageService.Quantity ?? 1;
                    var usedQuantity = 0;

                    // For subscriptions that have used services, mark some as used
                    if (subscription.Status == "Active" && subscription.UsedServices > 0)
                    {
                        // Randomly use 0-2 services depending on subscription
                        if (subscription.SubscriptionCode == "SUB-2025-002") // Premium - 2 used
                        {
                            usedQuantity = packageService.ServiceId % 2 == 0 ? 1 : 0; // Use some services
                        }
                        else if (subscription.SubscriptionCode == "SUB-2025-003") // VIP - 1 used
                        {
                            usedQuantity = packageService.ServiceId == 1 ? 1 : 0; // Use only first service
                        }
                    }
                    else if (subscription.Status == "Expired")
                    {
                        usedQuantity = totalQuantity; // All used up
                    }

                    var usage = new PackageServiceUsage
                    {
                        SubscriptionId = subscription.SubscriptionId,
                        ServiceId = packageService.ServiceId,
                        TotalAllowedQuantity = totalQuantity,
                        UsedQuantity = usedQuantity,
                        RemainingQuantity = totalQuantity - usedQuantity,
                        LastUsedDate = usedQuantity > 0 ? subscription.LastServiceDate?.ToDateTime(TimeOnly.MinValue) : null,
                        Notes = $"Auto-generated usage tracking for {packageService.Service.ServiceName}"
                    };

                    usageRecords.Add(usage);
                }
            }

            return usageRecords;
        }
    }
}
