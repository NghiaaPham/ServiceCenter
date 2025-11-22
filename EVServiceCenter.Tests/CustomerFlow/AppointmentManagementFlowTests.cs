using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.CustomerVehicles.Entities;
using EVServiceCenter.Core.Domains.AppointmentManagement.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.CarBrands.Entities;
using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;

namespace EVServiceCenter.Tests.CustomerFlow;

/// <summary>
/// Tests đầy đủ cho Appointment Management Flow - Phase 4
/// Đặt lịch, reschedule, complete với subscription
/// </summary>
public class AppointmentManagementFlowTests : TestBase
{
    private async Task<(Customer, CustomerVehicle, ServiceCenter)> SeedBasicDataAsync()
    {
        var customer = new Customer
        {
            CustomerCode = "CUST000001",
            FullName = "Test User",
            PhoneNumber = "0901234567",
            Email = "test@test.com",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        DbContext.Customers.Add(customer);

        var brand = new CarBrand { BrandName = "VinFast", IsActive = true };
        DbContext.CarBrands.Add(brand);
        await DbContext.SaveChangesAsync();

        var model = new CarModel { BrandId = brand.BrandId, ModelName = "VF 8", Year = 2024, IsActive = true };
        DbContext.CarModels.Add(model);
        await DbContext.SaveChangesAsync();

        var vehicle = new CustomerVehicle
        {
            CustomerId = customer.CustomerId,
            ModelId = model.ModelId,
            LicensePlate = "30A-12345",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        DbContext.CustomerVehicles.Add(vehicle);

        var serviceCenter = new ServiceCenter
        {
            CenterCode = "SC001",
            CenterName = "EV Service Center HN",
            Address = "123 ABC",
            PhoneNumber = "024123456",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        DbContext.ServiceCenters.Add(serviceCenter);
        await DbContext.SaveChangesAsync();

        return (customer, vehicle, serviceCenter);
    }

    [Fact]
    public async Task CreateAppointment_WithSubscription_ShouldLinkCorrectly()
    {
        // Arrange
        var (customer, vehicle, serviceCenter) = await SeedBasicDataAsync();

        // Create package and subscription
        var package = new MaintenancePackage
        {
            PackageCode = "PKG001",
            PackageName = "Gói 6 tháng",
            TotalPrice = 2000000,
            ValidityPeriod = 6,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        DbContext.MaintenancePackages.Add(package);
        await DbContext.SaveChangesAsync();

        var subscription = new CustomerPackageSubscription
        {
            CustomerId = customer.CustomerId,
            PackageId = package.PackageId,
            VehicleId = vehicle.VehicleId,
            SubscriptionCode = "SUB001",
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            Status = "Active",
            PaymentAmount = 1800000,
            CreatedDate = DateTime.UtcNow
        };
        DbContext.CustomerPackageSubscriptions.Add(subscription);
        await DbContext.SaveChangesAsync();

        // Act - Create appointment with subscription
        var appointment = new Appointment
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            ServiceCenterId = serviceCenter.CenterId,
            SubscriptionId = subscription.SubscriptionId,
            AppointmentCode = "APT001",
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            StatusId = 1,
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 },
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        // Assert
        var saved = await DbContext.Appointments
            .Include(a => a.Subscription)
            .FirstOrDefaultAsync(a => a.AppointmentCode == "APT001");

        saved.Should().NotBeNull();
        saved!.SubscriptionId.Should().Be(subscription.SubscriptionId);
        saved.Subscription.Should().NotBeNull();
        saved.Subscription!.SubscriptionCode.Should().Be("SUB001");
    }

    [Fact]
    public async Task RescheduleAppointment_ShouldCreateNewAppointment()
    {
        // Arrange
        var (customer, vehicle, serviceCenter) = await SeedBasicDataAsync();

        var oldAppointment = new Appointment
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            ServiceCenterId = serviceCenter.CenterId,
            AppointmentCode = "APT001",
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            StatusId = 1, // Pending
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 },
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Appointments.Add(oldAppointment);
        await DbContext.SaveChangesAsync();

        // Act - Reschedule
        // Step 1: Mark old as Rescheduled (StatusId = 6)
        oldAppointment.StatusId = 6;
        oldAppointment.UpdatedDate = DateTime.UtcNow;

        // Step 2: Create new appointment
        var newAppointment = new Appointment
        {
            CustomerId = oldAppointment.CustomerId,
            VehicleId = oldAppointment.VehicleId,
            ServiceCenterId = oldAppointment.ServiceCenterId,
            AppointmentCode = "APT002",
            AppointmentDate = DateTime.UtcNow.AddDays(2), // New date
            StatusId = 1, // Pending
            RescheduledFromId = oldAppointment.AppointmentId,
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 },
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Appointments.Add(newAppointment);
        await DbContext.SaveChangesAsync();

        // Assert
        var oldSaved = await DbContext.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentCode == "APT001");
        var newSaved = await DbContext.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentCode == "APT002");

        oldSaved.Should().NotBeNull();
        oldSaved!.StatusId.Should().Be(6, "Old appointment marked as Rescheduled");

        newSaved.Should().NotBeNull();
        newSaved!.StatusId.Should().Be(1, "New appointment is Pending");
        newSaved.RescheduledFromId.Should().Be(oldAppointment.AppointmentId);
    }

    [Fact]
    public async Task UpdateAppointment_WithPendingStatus_ShouldSucceed()
    {
        // Arrange
        var (customer, vehicle, serviceCenter) = await SeedBasicDataAsync();

        var appointment = new Appointment
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            ServiceCenterId = serviceCenter.CenterId,
            AppointmentCode = "APT001",
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            StatusId = 1, // Pending
            CustomerNotes = "Original notes",
            Priority = "Normal",
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 },
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        // Act - Update
        appointment.CustomerNotes = "Updated notes";
        appointment.Priority = "Urgent";
        appointment.UpdatedDate = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();

        var updated = await DbContext.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentCode == "APT001");

        // Assert
        updated.Should().NotBeNull();
        updated!.CustomerNotes.Should().Be("Updated notes");
        updated.Priority.Should().Be("Urgent");
        updated.UpdatedDate.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteAppointment_WithSubscription_ShouldDeductUsage()
    {
        // Arrange
        var (customer, vehicle, serviceCenter) = await SeedBasicDataAsync();

        // Create package, subscription, and usage
        var package = new MaintenancePackage
        {
            PackageCode = "PKG001",
            PackageName = "Gói 6 tháng",
            TotalPrice = 2000000,
            ValidityPeriod = 6,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        DbContext.MaintenancePackages.Add(package);
        await DbContext.SaveChangesAsync();

        var subscription = new CustomerPackageSubscription
        {
            CustomerId = customer.CustomerId,
            PackageId = package.PackageId,
            VehicleId = vehicle.VehicleId,
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

        // Create appointment
        var appointment = new Appointment
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            ServiceCenterId = serviceCenter.CenterId,
            SubscriptionId = subscription.SubscriptionId,
            AppointmentCode = "APT001",
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            StatusId = 3, // InProgress
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 },
            CreatedDate = DateTime.UtcNow
        };
        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        // Act - Complete appointment and deduct usage
        appointment.StatusId = 5; // Completed (giả sử 5 là Completed)
        appointment.CompletedDate = DateTime.UtcNow;
        appointment.CompletedBy = 1;

        // Deduct usage
        usage.UsedQuantity += 1;
        usage.RemainingQuantity -= 1;
        usage.LastUsedDate = DateTime.UtcNow;
        usage.LastUsedAppointmentId = appointment.AppointmentId;

        await DbContext.SaveChangesAsync();

        // Assert
        var completedAppointment = await DbContext.Appointments
            .FirstOrDefaultAsync(a => a.AppointmentCode == "APT001");
        var updatedUsage = await DbContext.PackageServiceUsages
            .FirstOrDefaultAsync(u => u.SubscriptionId == subscription.SubscriptionId);

        completedAppointment.Should().NotBeNull();
        completedAppointment!.StatusId.Should().Be(5);
        completedAppointment.CompletedDate.Should().NotBeNull();

        updatedUsage.Should().NotBeNull();
        updatedUsage!.UsedQuantity.Should().Be(1, "Đã trừ 1 lượt");
        updatedUsage.RemainingQuantity.Should().Be(2, "Còn 2 lượt");
        updatedUsage.LastUsedAppointmentId.Should().Be(appointment.AppointmentId);
    }

    [Fact]
    public async Task GetUpcomingAppointments_ShouldReturnFutureAppointmentsOnly()
    {
        // Arrange
        var (customer, vehicle, serviceCenter) = await SeedBasicDataAsync();

        var appointments = new List<Appointment>
        {
            new Appointment { CustomerId = customer.CustomerId, VehicleId = vehicle.VehicleId, ServiceCenterId = serviceCenter.CenterId, AppointmentCode = "APT001", AppointmentDate = DateTime.UtcNow.AddDays(1), StatusId = 1, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, CreatedDate = DateTime.UtcNow },
            new Appointment { CustomerId = customer.CustomerId, VehicleId = vehicle.VehicleId, ServiceCenterId = serviceCenter.CenterId, AppointmentCode = "APT002", AppointmentDate = DateTime.UtcNow.AddDays(2), StatusId = 2, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, CreatedDate = DateTime.UtcNow },
            new Appointment { CustomerId = customer.CustomerId, VehicleId = vehicle.VehicleId, ServiceCenterId = serviceCenter.CenterId, AppointmentCode = "APT003", AppointmentDate = DateTime.UtcNow.AddDays(-1), StatusId = 5, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, CreatedDate = DateTime.UtcNow.AddDays(-2) }
        };

        DbContext.Appointments.AddRange(appointments);
        await DbContext.SaveChangesAsync();

        // Act
        var now = DateTime.UtcNow;
        var upcomingAppointments = await DbContext.Appointments
            .Where(a => a.CustomerId == customer.CustomerId && a.AppointmentDate > now)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        // Assert
        upcomingAppointments.Should().NotBeNull();
        upcomingAppointments.Should().HaveCount(2);
        upcomingAppointments.Should().OnlyContain(a => a.AppointmentDate > now);
        upcomingAppointments.Should().NotContain(a => a.AppointmentCode == "APT003");
    }

    [Fact]
    public async Task AddMultipleServicesToAppointment_ShouldCalculateTotalPrice()
    {
        // Arrange
        var (customer, vehicle, serviceCenter) = await SeedBasicDataAsync();

        var services = new List<MaintenanceService>
        {
            new MaintenanceService { ServiceCode = "SRV001", ServiceName = "Thay dầu", BasePrice = 500000, IsActive = true },
            new MaintenanceService { ServiceCode = "SRV002", ServiceName = "Kiểm tra phanh", BasePrice = 300000, IsActive = true },
            new MaintenanceService { ServiceCode = "SRV003", ServiceName = "Kiểm tra lốp", BasePrice = 200000, IsActive = true }
        };
        DbContext.MaintenanceServices.AddRange(services);
        await DbContext.SaveChangesAsync();

        var appointment = new Appointment
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            ServiceCenterId = serviceCenter.CenterId,
            AppointmentCode = "APT001",
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            StatusId = 1,
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 },
            CreatedDate = DateTime.UtcNow
        };
        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        // Act - Add services
        var appointmentServices = new List<AppointmentService>
        {
            new AppointmentService { AppointmentId = appointment.AppointmentId, ServiceId = services[0].ServiceId, ServiceSource = "Regular", Price = 500000, EstimatedTime = 60, CreatedDate = DateTime.UtcNow },
            new AppointmentService { AppointmentId = appointment.AppointmentId, ServiceId = services[1].ServiceId, ServiceSource = "Regular", Price = 300000, EstimatedTime = 30, CreatedDate = DateTime.UtcNow },
            new AppointmentService { AppointmentId = appointment.AppointmentId, ServiceId = services[2].ServiceId, ServiceSource = "Regular", Price = 200000, EstimatedTime = 20, CreatedDate = DateTime.UtcNow }
        };

        DbContext.AppointmentServices.AddRange(appointmentServices);
        await DbContext.SaveChangesAsync();

        // Calculate total
        var totalPrice = appointmentServices.Sum(s => s.Price);
        appointment.EstimatedCost = totalPrice;
        await DbContext.SaveChangesAsync();

        // Assert
        var saved = await DbContext.Appointments
            .Include(a => a.AppointmentServices)
            .FirstOrDefaultAsync(a => a.AppointmentCode == "APT001");

        saved.Should().NotBeNull();
        saved!.AppointmentServices.Should().HaveCount(3);
        saved.AppointmentServices.Sum(s => s.Price).Should().Be(1000000);
        saved.EstimatedCost.Should().Be(1000000);
    }

    [Fact]
    public async Task GenerateAppointmentCode_ShouldFollowCorrectFormat()
    {
        // Arrange
        var (customer, vehicle, serviceCenter) = await SeedBasicDataAsync();

        var today = DateTime.UtcNow;
        var datePrefix = today.ToString("yyyyMMdd");

        var existingAppointments = new List<Appointment>
        {
            new Appointment { CustomerId = customer.CustomerId, VehicleId = vehicle.VehicleId, ServiceCenterId = serviceCenter.CenterId, AppointmentCode = $"APT{datePrefix}001", AppointmentDate = today.AddDays(1), StatusId = 1, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, CreatedDate = today },
            new Appointment { CustomerId = customer.CustomerId, VehicleId = vehicle.VehicleId, ServiceCenterId = serviceCenter.CenterId, AppointmentCode = $"APT{datePrefix}002", AppointmentDate = today.AddDays(1), StatusId = 1, RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, CreatedDate = today }
        };

        DbContext.Appointments.AddRange(existingAppointments);
        await DbContext.SaveChangesAsync();

        // Act
        var lastCode = await DbContext.Appointments
            .Where(a => a.AppointmentCode.StartsWith($"APT{datePrefix}"))
            .OrderByDescending(a => a.AppointmentCode)
            .Select(a => a.AppointmentCode)
            .FirstOrDefaultAsync();

        var lastNumber = int.Parse(lastCode!.Substring(11));
        var nextCode = $"APT{datePrefix}{(lastNumber + 1):D3}";

        // Assert
        lastCode.Should().Be($"APT{datePrefix}002");
        nextCode.Should().Be($"APT{datePrefix}003");
        nextCode.Should().MatchRegex(@"^APT\d{8}\d{3}$");
    }
}
