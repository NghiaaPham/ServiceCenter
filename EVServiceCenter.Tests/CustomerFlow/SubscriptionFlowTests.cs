using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.CarModels.Entities;

namespace EVServiceCenter.Tests.CustomerFlow;

/// <summary>
/// Tests cho Subscription Management Flow - Phase 3
/// Mua gói, theo dõi usage, hủy subscription
/// </summary>
public class SubscriptionFlowTests : TestBase
{
    private async Task SeedTestDataAsync()
    {
        // Customer
        var customer = new Customer
        {
            CustomerId = 1,
            CustomerCode = "CUST000001",
            FullName = "Test User",
            PhoneNumber = "0901234567",
            Email = "test@test.com",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        DbContext.Customers.Add(customer);

        // Car Brand & Model
        var brand = new CarBrand { BrandId = 1, BrandName = "VinFast", IsActive = true };
        DbContext.CarBrands.Add(brand);
        await DbContext.SaveChangesAsync();

        var model = new CarModel { ModelId = 1, BrandId = 1, ModelName = "VF 8", Year = 2024, IsActive = true };
        DbContext.CarModels.Add(model);
        await DbContext.SaveChangesAsync();

        // Vehicle
        var vehicle = new CustomerVehicle
        {
            VehicleId = 1,
            CustomerId = 1,
            ModelId = 1,
            LicensePlate = "30A-12345",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        DbContext.CustomerVehicles.Add(vehicle);

        // Maintenance Services
        var services = new List<MaintenanceService>
        {
            new MaintenanceService { ServiceId = 1, ServiceCode = "SRV001", ServiceName = "Thay dầu động cơ", BasePrice = 500000, IsActive = true },
            new MaintenanceService { ServiceId = 2, ServiceCode = "SRV002", ServiceName = "Kiểm tra phanh", BasePrice = 300000, IsActive = true },
            new MaintenanceService { ServiceId = 3, ServiceCode = "SRV003", ServiceName = "Kiểm tra lốp", BasePrice = 200000, IsActive = true }
        };
        DbContext.MaintenanceServices.AddRange(services);

        // Maintenance Package
        var package = new MaintenancePackage
        {
            PackageId = 1,
            PackageCode = "PKG001",
            PackageName = "Gói bảo dưỡng 6 tháng",
            Description = "Gói bảo dưỡng toàn diện",
            TotalPrice = 2000000,
            DiscountPercent = 10,
            ValidityPeriod = 6,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        DbContext.MaintenancePackages.Add(package);
        await DbContext.SaveChangesAsync();

        // Package Services
        var packageServices = new List<PackageService>
        {
            new PackageService { PackageId = 1, ServiceId = 1, Quantity = 3 },
            new PackageService { PackageId = 1, ServiceId = 2, Quantity = 2 },
            new PackageService { PackageId = 1, ServiceId = 3, Quantity = 2 }
        };
        DbContext.PackageServices.AddRange(packageServices);
        await DbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GetMaintenancePackages_ShouldReturnActivePackages()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var packages = await DbContext.MaintenancePackages
            .Where(p => p.IsActive == true)
            .Include(p => p.PackageServices)
            .ThenInclude(ps => ps.Service)
            .ToListAsync();

        // Assert
        packages.Should().NotBeNull();
        packages.Should().HaveCount(1);
        packages[0].PackageName.Should().Be("Gói bảo dưỡng 6 tháng");
        packages[0].PackageServices.Should().HaveCount(3);
        packages[0].TotalPrice.Should().Be(2000000);
        packages[0].DiscountPercent.Should().Be(10);
    }

    [Fact]
    public async Task PurchaseSubscription_ShouldCreateSubscriptionAndUsageRecords()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act - Create Subscription
        var subscription = new CustomerPackageSubscription
        {
            CustomerId = 1,
            PackageId = 1,
            VehicleId = 1,
            SubscriptionCode = "SUB20250115001",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            Status = "Active",
            OriginalPrice = 2000000,
            DiscountPercent = 10,
            DiscountAmount = 200000,
            PaymentAmount = 1800000,
            PaymentMethodId = 1,
            PurchaseDate = DateTime.UtcNow,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.CustomerPackageSubscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        // Create Usage Records
        var usages = new List<PackageServiceUsage>
        {
            new PackageServiceUsage
            {
                SubscriptionId = subscription.SubscriptionId,
                ServiceId = 1,
                TotalAllowedQuantity = 3,
                UsedQuantity = 0,
                RemainingQuantity = 3
            },
            new PackageServiceUsage
            {
                SubscriptionId = subscription.SubscriptionId,
                ServiceId = 2,
                TotalAllowedQuantity = 2,
                UsedQuantity = 0,
                RemainingQuantity = 2
            },
            new PackageServiceUsage
            {
                SubscriptionId = subscription.SubscriptionId,
                ServiceId = 3,
                TotalAllowedQuantity = 2,
                UsedQuantity = 0,
                RemainingQuantity = 2
            }
        };

        DbContext.PackageServiceUsages.AddRange(usages);
        await DbContext.SaveChangesAsync();

        // Assert
        var savedSubscription = await DbContext.CustomerPackageSubscriptions
            .Include(s => s.PackageServiceUsages)
            .FirstOrDefaultAsync(s => s.SubscriptionCode == "SUB20250115001");

        savedSubscription.Should().NotBeNull();
        savedSubscription!.Status.Should().Be("Active");
        savedSubscription.PaymentAmount.Should().Be(1800000);
        savedSubscription.PackageServiceUsages.Should().HaveCount(3);
        savedSubscription.PackageServiceUsages.Should().OnlyContain(u => u.UsedQuantity == 0);
    }

    [Fact]
    public async Task GenerateSubscriptionCode_ShouldFollowCorrectFormat()
    {
        // Arrange
        await SeedTestDataAsync();
        var today = DateTime.UtcNow;
        var datePrefix = today.ToString("yyyyMMdd");

        var existingSubscriptions = new List<CustomerPackageSubscription>
        {
            new CustomerPackageSubscription
            {
                CustomerId = 1,
                PackageId = 1,
                VehicleId = 1,
                SubscriptionCode = $"SUB{datePrefix}001",
                StartDate = DateOnly.FromDateTime(today),
                ExpirationDate = DateOnly.FromDateTime(today.AddMonths(6)),
                Status = "Active",
                PaymentAmount = 1800000,
                CreatedDate = today
            },
            new CustomerPackageSubscription
            {
                CustomerId = 1,
                PackageId = 1,
                VehicleId = 1,
                SubscriptionCode = $"SUB{datePrefix}002",
                StartDate = DateOnly.FromDateTime(today),
                ExpirationDate = DateOnly.FromDateTime(today.AddMonths(6)),
                Status = "Active",
                PaymentAmount = 1800000,
                CreatedDate = today
            }
        };

        DbContext.CustomerPackageSubscriptions.AddRange(existingSubscriptions);
        await DbContext.SaveChangesAsync();

        // Act
        var lastCode = await DbContext.CustomerPackageSubscriptions
            .Where(s => s.SubscriptionCode.StartsWith($"SUB{datePrefix}"))
            .OrderByDescending(s => s.SubscriptionCode)
            .Select(s => s.SubscriptionCode)
            .FirstOrDefaultAsync();

        var lastNumber = int.Parse(lastCode!.Substring(11));
        var nextCode = $"SUB{datePrefix}{(lastNumber + 1):D3}";

        // Assert
        lastCode.Should().Be($"SUB{datePrefix}002");
        nextCode.Should().Be($"SUB{datePrefix}003");
        nextCode.Should().MatchRegex(@"^SUB\d{8}\d{3}$");
    }

    [Fact]
    public async Task GetMySubscriptions_ShouldReturnOnlyCustomerSubscriptions()
    {
        // Arrange
        await SeedTestDataAsync();

        // Add another customer
        var customer2 = new Customer
        {
            CustomerId = 2,
            CustomerCode = "CUST000002",
            FullName = "Customer 2",
            PhoneNumber = "0987654321",
            Email = "customer2@test.com",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        DbContext.Customers.Add(customer2);
        await DbContext.SaveChangesAsync();

        var subscriptions = new List<CustomerPackageSubscription>
        {
            new CustomerPackageSubscription { CustomerId = 1, PackageId = 1, VehicleId = 1, SubscriptionCode = "SUB001", StartDate = DateOnly.FromDateTime(DateTime.UtcNow), ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)), Status = "Active", PaymentAmount = 1800000, CreatedDate = DateTime.UtcNow },
            new CustomerPackageSubscription { CustomerId = 1, PackageId = 1, VehicleId = 1, SubscriptionCode = "SUB002", StartDate = DateOnly.FromDateTime(DateTime.UtcNow), ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)), Status = "Active", PaymentAmount = 1800000, CreatedDate = DateTime.UtcNow },
            new CustomerPackageSubscription { CustomerId = 2, PackageId = 1, VehicleId = 1, SubscriptionCode = "SUB003", StartDate = DateOnly.FromDateTime(DateTime.UtcNow), ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)), Status = "Active", PaymentAmount = 1800000, CreatedDate = DateTime.UtcNow }
        };

        DbContext.CustomerPackageSubscriptions.AddRange(subscriptions);
        await DbContext.SaveChangesAsync();

        // Act
        var customer1Subscriptions = await DbContext.CustomerPackageSubscriptions
            .Where(s => s.CustomerId == 1)
            .ToListAsync();

        // Assert
        customer1Subscriptions.Should().NotBeNull();
        customer1Subscriptions.Should().HaveCount(2);
        customer1Subscriptions.Should().OnlyContain(s => s.CustomerId == 1);
        customer1Subscriptions.Should().NotContain(s => s.SubscriptionCode == "SUB003");
    }

    [Fact]
    public async Task GetSubscriptionUsage_ShouldShowCorrectRemainingQuantity()
    {
        // Arrange
        await SeedTestDataAsync();

        var subscription = new CustomerPackageSubscription
        {
            CustomerId = 1,
            PackageId = 1,
            VehicleId = 1,
            SubscriptionCode = "SUB001",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            Status = "Active",
            PaymentAmount = 1800000,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.CustomerPackageSubscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        var usage = new PackageServiceUsage
        {
            SubscriptionId = subscription.SubscriptionId,
            ServiceId = 1,
            TotalAllowedQuantity = 3,
            UsedQuantity = 1,
            RemainingQuantity = 2,
            LastUsedDate = DateTime.UtcNow
        };

        DbContext.PackageServiceUsages.Add(usage);
        await DbContext.SaveChangesAsync();

        // Act
        var savedUsage = await DbContext.PackageServiceUsages
            .FirstOrDefaultAsync(u => u.SubscriptionId == subscription.SubscriptionId);

        // Assert
        savedUsage.Should().NotBeNull();
        savedUsage!.TotalAllowedQuantity.Should().Be(3);
        savedUsage.UsedQuantity.Should().Be(1);
        savedUsage.RemainingQuantity.Should().Be(2);
        savedUsage.LastUsedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task UseSubscriptionService_ShouldDecrementRemainingQuantity()
    {
        // Arrange
        await SeedTestDataAsync();

        var subscription = new CustomerPackageSubscription
        {
            CustomerId = 1,
            PackageId = 1,
            VehicleId = 1,
            SubscriptionCode = "SUB001",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            Status = "Active",
            PaymentAmount = 1800000,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.CustomerPackageSubscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        var usage = new PackageServiceUsage
        {
            SubscriptionId = subscription.SubscriptionId,
            ServiceId = 1,
            TotalAllowedQuantity = 3,
            UsedQuantity = 0,
            RemainingQuantity = 3
        };

        DbContext.PackageServiceUsages.Add(usage);
        await DbContext.SaveChangesAsync();

        // Act - Use service
        usage.UsedQuantity += 1;
        usage.RemainingQuantity -= 1;
        usage.LastUsedDate = DateTime.UtcNow;
        usage.LastUsedAppointmentId = 1;
        await DbContext.SaveChangesAsync();

        var updatedUsage = await DbContext.PackageServiceUsages
            .FirstOrDefaultAsync(u => u.SubscriptionId == subscription.SubscriptionId);

        // Assert
        updatedUsage.Should().NotBeNull();
        updatedUsage!.UsedQuantity.Should().Be(1);
        updatedUsage.RemainingQuantity.Should().Be(2);
        updatedUsage.LastUsedAppointmentId.Should().Be(1);
    }

    [Fact]
    public async Task CancelSubscription_ShouldUpdateStatus()
    {
        // Arrange
        await SeedTestDataAsync();

        var subscription = new CustomerPackageSubscription
        {
            CustomerId = 1,
            PackageId = 1,
            VehicleId = 1,
            SubscriptionCode = "SUB001",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            Status = "Active",
            PaymentAmount = 1800000,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.CustomerPackageSubscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        // Act - Cancel
        subscription.Status = "Cancelled";
        subscription.CancellationReason = "Không còn nhu cầu sử dụng";
        subscription.CancelledDate = DateOnly.FromDateTime(DateTime.UtcNow);
        await DbContext.SaveChangesAsync();

        var cancelled = await DbContext.CustomerPackageSubscriptions
            .FirstOrDefaultAsync(s => s.SubscriptionId == subscription.SubscriptionId);

        // Assert
        cancelled.Should().NotBeNull();
        cancelled!.Status.Should().Be("Cancelled");
        cancelled.CancellationReason.Should().Be("Không còn nhu cầu sử dụng");
        cancelled.CancelledDate.Should().NotBeNull();
    }

    [Fact]
    public async Task ExpiredSubscription_ShouldBeDetected()
    {
        // Arrange
        await SeedTestDataAsync();

        var subscription = new CustomerPackageSubscription
        {
            CustomerId = 1,
            PackageId = 1,
            VehicleId = 1,
            SubscriptionCode = "SUB001",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-7)),
            ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), // Expired
            Status = "Active",
            PaymentAmount = 1800000,
            CreatedDate = DateTime.UtcNow.AddMonths(-7)
        };

        DbContext.CustomerPackageSubscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        // Act
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isExpired = subscription.ExpirationDate < today;

        if (isExpired && subscription.Status == "Active")
        {
            subscription.Status = "Expired";
            await DbContext.SaveChangesAsync();
        }

        // Assert
        var expiredSub = await DbContext.CustomerPackageSubscriptions
            .FirstOrDefaultAsync(s => s.SubscriptionId == subscription.SubscriptionId);

        expiredSub.Should().NotBeNull();
        expiredSub!.Status.Should().Be("Expired");
    }
}
