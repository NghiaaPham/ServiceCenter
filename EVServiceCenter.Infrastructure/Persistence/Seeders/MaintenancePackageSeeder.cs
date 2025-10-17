using EVServiceCenter.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EVServiceCenter.Infrastructure.Persistence.Seeders
{
    /// <summary>
    /// Seeder for MaintenancePackages data
    /// Creates 3 default packages: Basic, Premium, VIP
    /// </summary>
    public class MaintenancePackageSeeder
    {
        private readonly EVDbContext _context;
        private readonly ILogger<MaintenancePackageSeeder> _logger;

        public MaintenancePackageSeeder(
            EVDbContext context,
            ILogger<MaintenancePackageSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Seed MaintenancePackages and PackageServices
        /// </summary>
        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting MaintenancePackage seeding...");

            // Check if packages already exist
            var existingPackages = await _context.MaintenancePackages
                .Where(p => p.PackageCode.StartsWith("PKG-"))
                .ToListAsync();

            if (existingPackages.Any())
            {
                _logger.LogInformation("MaintenancePackages already seeded. Skipping...");
                return;
            }

            // Get services
            var services = await GetServicesAsync();
            if (services.Count < 4)
            {
                _logger.LogWarning($"Not enough services found ({services.Count}). Need at least 4 services.");
                return;
            }

            // Create packages
            var packages = CreatePackages();

            // Add packages to context
            await _context.MaintenancePackages.AddRangeAsync(packages);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created {Count} maintenance packages", packages.Count);

            // Create package-service relations
            var packageServices = CreatePackageServices(packages, services);
            await _context.PackageServices.AddRangeAsync(packageServices);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created {Count} package-service relations", packageServices.Count);
            _logger.LogInformation("MaintenancePackage seeding completed successfully!");
        }

        private List<MaintenancePackage> CreatePackages()
        {
            return new List<MaintenancePackage>
            {
                // Package 1: Basic
                new MaintenancePackage
                {
                    PackageCode = "PKG-BASIC-2025",
                    PackageName = "Gói Bảo Dưỡng Cơ Bản",
                    Description = "Gói bảo dưỡng định kỳ dành cho xe điện, bao gồm các dịch vụ cơ bản: kiểm tra pin, phanh, lốp xe. Phù hợp cho việc bảo dưỡng thường xuyên.",
                    ValidityPeriod = 365,        // 1 năm
                    ValidityMileage = 10000,     // 10,000 km
                    TotalPrice = 2000000,        // 2 triệu VND
                    DiscountPercent = 20,        // Giảm 20%
                    ImageUrl = "/images/packages/basic-package.jpg",
                    IsPopular = false,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Package 2: Premium
                new MaintenancePackage
                {
                    PackageCode = "PKG-PREMIUM-2025",
                    PackageName = "Gói Bảo Dưỡng Cao Cấp",
                    Description = "Gói bảo dưỡng toàn diện với các dịch vụ chuyên sâu: kiểm tra hệ thống điện, pin, phanh, lốp, điều hòa. Bao gồm 2 lần rửa xe miễn phí.",
                    ValidityPeriod = 365,        // 1 năm
                    ValidityMileage = 15000,     // 15,000 km
                    TotalPrice = 4500000,        // 4.5 triệu VND
                    DiscountPercent = 25,        // Giảm 25%
                    ImageUrl = "/images/packages/premium-package.jpg",
                    IsPopular = true,            // Popular
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                },

                // Package 3: VIP
                new MaintenancePackage
                {
                    PackageCode = "PKG-VIP-2025",
                    PackageName = "Gói Bảo Dưỡng VIP",
                    Description = "Gói bảo dưỡng cao cấp nhất, ưu tiên phục vụ, bảo dưỡng toàn diện mọi hệ thống. Bao gồm rửa xe không giới hạn, chăm sóc nội thất định kỳ.",
                    ValidityPeriod = 730,        // 2 năm
                    ValidityMileage = 30000,     // 30,000 km
                    TotalPrice = 8000000,        // 8 triệu VND
                    DiscountPercent = 30,        // Giảm 30%
                    ImageUrl = "/images/packages/vip-package.jpg",
                    IsPopular = true,            // Popular
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                }
            };
        }

        private async Task<Dictionary<string, int>> GetServicesAsync()
        {
            // Get any 8 services from database (fallback to whatever exists)
            var allServices = await _context.MaintenanceServices
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.ServiceId)
                .Take(8)
                .ToListAsync();

            if (allServices.Count < 4)
            {
                _logger.LogWarning($"Only found {allServices.Count} services, need at least 4");
                return new Dictionary<string, int>();
            }

            // Map to expected keys (use first N services for each type)
            var serviceDict = new Dictionary<string, int>();
            var serviceKeys = new[] { "SRV-OIL-001", "SRV-BRAKE-001", "SRV-BATTERY-001", "SRV-TIRE-001",
                                     "SRV-AC-001", "SRV-WASH-001", "SRV-INTERIOR-001", "SRV-ENGINE-001" };

            for (int i = 0; i < allServices.Count && i < serviceKeys.Length; i++)
            {
                serviceDict[serviceKeys[i]] = allServices[i].ServiceId;
            }

            _logger.LogInformation($"Found {serviceDict.Count} services for packages");
            return serviceDict;
        }

        private List<PackageService> CreatePackageServices(
            List<MaintenancePackage> packages,
            Dictionary<string, int> services)
        {
            var packageServices = new List<PackageService>();

            // Package 1: Basic (4 services, 2 times each)
            var basicPackage = packages.First(p => p.PackageCode == "PKG-BASIC-2025");
            packageServices.AddRange(new[]
            {
                new PackageService
                {
                    PackageId = basicPackage.PackageId,
                    ServiceId = services["SRV-OIL-001"],
                    Quantity = 2,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = basicPackage.PackageId,
                    ServiceId = services["SRV-BRAKE-001"],
                    Quantity = 2,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = basicPackage.PackageId,
                    ServiceId = services["SRV-BATTERY-001"],
                    Quantity = 2,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = basicPackage.PackageId,
                    ServiceId = services["SRV-TIRE-001"],
                    Quantity = 2,
                    IncludedInPackage = true
                }
            });

            // Package 2: Premium (6 services, 2-3 times)
            var premiumPackage = packages.First(p => p.PackageCode == "PKG-PREMIUM-2025");
            packageServices.AddRange(new[]
            {
                new PackageService
                {
                    PackageId = premiumPackage.PackageId,
                    ServiceId = services["SRV-OIL-001"],
                    Quantity = 3,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = premiumPackage.PackageId,
                    ServiceId = services["SRV-BRAKE-001"],
                    Quantity = 3,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = premiumPackage.PackageId,
                    ServiceId = services["SRV-BATTERY-001"],
                    Quantity = 3,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = premiumPackage.PackageId,
                    ServiceId = services["SRV-TIRE-001"],
                    Quantity = 3,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = premiumPackage.PackageId,
                    ServiceId = services["SRV-AC-001"],
                    Quantity = 2,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = premiumPackage.PackageId,
                    ServiceId = services["SRV-WASH-001"],
                    Quantity = 2,
                    IncludedInPackage = true
                }
            });

            // Package 3: VIP (8 services, 4-99 times)
            var vipPackage = packages.First(p => p.PackageCode == "PKG-VIP-2025");
            packageServices.AddRange(new[]
            {
                new PackageService
                {
                    PackageId = vipPackage.PackageId,
                    ServiceId = services["SRV-OIL-001"],
                    Quantity = 5,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = vipPackage.PackageId,
                    ServiceId = services["SRV-BRAKE-001"],
                    Quantity = 5,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = vipPackage.PackageId,
                    ServiceId = services["SRV-BATTERY-001"],
                    Quantity = 5,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = vipPackage.PackageId,
                    ServiceId = services["SRV-TIRE-001"],
                    Quantity = 5,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = vipPackage.PackageId,
                    ServiceId = services["SRV-AC-001"],
                    Quantity = 4,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = vipPackage.PackageId,
                    ServiceId = services["SRV-ENGINE-001"],
                    Quantity = 4,
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = vipPackage.PackageId,
                    ServiceId = services["SRV-WASH-001"],
                    Quantity = 99, // Unlimited
                    IncludedInPackage = true
                },
                new PackageService
                {
                    PackageId = vipPackage.PackageId,
                    ServiceId = services["SRV-INTERIOR-001"],
                    Quantity = 4,
                    IncludedInPackage = true
                }
            });

            return packageServices;
        }
    }
}
