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

namespace EVServiceCenter.Tests.CustomerFlow;

/// <summary>
/// Basic Customer Flow Tests - Version cực kỳ đơn giản
/// Chỉ test các trường hợp cơ bản nhất để demo cho giảng viên
/// </summary>
public class BasicCustomerFlowTests : TestBase
{
    [Fact]
    public async Task CreateCustomer_ShouldSucceed()
    {
        // Arrange & Act
        var customer = new Customer
        {
            CustomerCode = "CUST000001",
            FullName = "Nguyễn Văn Test",
            PhoneNumber = "0901234567",
            Email = "test@example.com",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Customers.Add(customer);
        await DbContext.SaveChangesAsync();

        // Assert
        var saved = await DbContext.Customers.FirstOrDefaultAsync(c => c.Email == "test@example.com");
        saved.Should().NotBeNull();
        saved!.FullName.Should().Be("Nguyễn Văn Test");
        saved.CustomerCode.Should().Be("CUST000001");
    }

    [Fact]
    public async Task CreateVehicle_ShouldSucceed()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerCode = "CUST000001",
            FullName = "Test User",
            Email = "test@test.com",
            PhoneNumber = "0901234567",
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

        // Act
        var vehicle = new CustomerVehicle
        {
            CustomerId = customer.CustomerId,
            ModelId = model.ModelId,
            LicensePlate = "30A-12345",
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        DbContext.CustomerVehicles.Add(vehicle);
        await DbContext.SaveChangesAsync();

        // Assert
        var saved = await DbContext.CustomerVehicles
            .Include(v => v.Model)
            .ThenInclude(m => m!.Brand)
            .FirstOrDefaultAsync(v => v.LicensePlate == "30A-12345");

        saved.Should().NotBeNull();
        saved!.Model.Should().NotBeNull();
        saved.Model!.ModelName.Should().Be("VF 8");
        saved.Model.Brand.Should().NotBeNull();
        saved.Model.Brand!.BrandName.Should().Be("VinFast");
    }

    [Fact]
    public async Task CreateAppointment_ShouldSucceed()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerCode = "CUST000001",
            FullName = "Test User",
            Email = "test@test.com",
            PhoneNumber = "0901234567",
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

        // Act
        var appointment = new Appointment
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            ServiceCenterId = serviceCenter.CenterId,
            AppointmentCode = "APT20250115001",
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            StatusId = 1,
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, // Mock RowVersion
            CreatedDate = DateTime.UtcNow
        };

        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        // Assert
        var saved = await DbContext.Appointments
            .Include(a => a.Customer)
            .Include(a => a.Vehicle)
            .Include(a => a.ServiceCenter)
            .FirstOrDefaultAsync(a => a.AppointmentCode == "APT20250115001");

        saved.Should().NotBeNull();
        saved!.Customer.Should().NotBeNull();
        saved.Customer!.FullName.Should().Be("Test User");
        saved.Vehicle.Should().NotBeNull();
        saved.ServiceCenter.Should().NotBeNull();
    }

    [Fact]
    public async Task AddServicesToAppointment_ShouldSucceed()
    {
        // Arrange
        var customer = new Customer { CustomerCode = "CUST001", FullName = "Test", Email = "test@test.com", PhoneNumber = "0901234567", IsActive = true, CreatedDate = DateTime.UtcNow };
        DbContext.Customers.Add(customer);

        var brand = new CarBrand { BrandName = "VinFast", IsActive = true };
        DbContext.CarBrands.Add(brand);
        await DbContext.SaveChangesAsync();

        var model = new CarModel { BrandId = brand.BrandId, ModelName = "VF 8", Year = 2024, IsActive = true };
        DbContext.CarModels.Add(model);
        await DbContext.SaveChangesAsync();

        var vehicle = new CustomerVehicle { CustomerId = customer.CustomerId, ModelId = model.ModelId, LicensePlate = "30A-123", IsActive = true, CreatedDate = DateTime.UtcNow };
        DbContext.CustomerVehicles.Add(vehicle);

        var serviceCenter = new ServiceCenter { CenterCode = "SC001", CenterName = "Center", Address = "123", PhoneNumber = "024123", IsActive = true, CreatedDate = DateTime.UtcNow };
        DbContext.ServiceCenters.Add(serviceCenter);

        var service1 = new MaintenanceService { ServiceCode = "SRV001", ServiceName = "Thay dầu", BasePrice = 500000, IsActive = true };
        var service2 = new MaintenanceService { ServiceCode = "SRV002", ServiceName = "Kiểm tra phanh", BasePrice = 300000, IsActive = true };
        DbContext.MaintenanceServices.AddRange(service1, service2);
        await DbContext.SaveChangesAsync();

        var appointment = new Appointment
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            ServiceCenterId = serviceCenter.CenterId,
            AppointmentCode = "APT001",
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            StatusId = 1,
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, // Mock RowVersion
            CreatedDate = DateTime.UtcNow
        };
        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        // Act
        var appointmentServices = new List<AppointmentService>
        {
            new AppointmentService
            {
                AppointmentId = appointment.AppointmentId,
                ServiceId = service1.ServiceId,
                ServiceSource = "Regular",
                Price = 500000,
                EstimatedTime = 60,
                CreatedDate = DateTime.UtcNow
            },
            new AppointmentService
            {
                AppointmentId = appointment.AppointmentId,
                ServiceId = service2.ServiceId,
                ServiceSource = "Regular",
                Price = 300000,
                EstimatedTime = 30,
                CreatedDate = DateTime.UtcNow
            }
        };

        DbContext.AppointmentServices.AddRange(appointmentServices);
        await DbContext.SaveChangesAsync();

        // Assert
        var saved = await DbContext.Appointments
            .Include(a => a.AppointmentServices)
            .ThenInclude(s => s.Service)
            .FirstOrDefaultAsync(a => a.AppointmentCode == "APT001");

        saved.Should().NotBeNull();
        saved!.AppointmentServices.Should().HaveCount(2);
        saved.AppointmentServices.Sum(s => s.Price).Should().Be(800000);
    }

    [Fact]
    public async Task GetCustomerVehicles_ShouldReturnOnlyOwnVehicles()
    {
        // Arrange
        var customer1 = new Customer { CustomerCode = "CUST001", FullName = "Customer 1", Email = "c1@test.com", PhoneNumber = "0901", IsActive = true, CreatedDate = DateTime.UtcNow };
        var customer2 = new Customer { CustomerCode = "CUST002", FullName = "Customer 2", Email = "c2@test.com", PhoneNumber = "0902", IsActive = true, CreatedDate = DateTime.UtcNow };
        DbContext.Customers.AddRange(customer1, customer2);

        var brand = new CarBrand { BrandName = "VinFast", IsActive = true };
        DbContext.CarBrands.Add(brand);
        await DbContext.SaveChangesAsync();

        var model = new CarModel { BrandId = brand.BrandId, ModelName = "VF 8", Year = 2024, IsActive = true };
        DbContext.CarModels.Add(model);
        await DbContext.SaveChangesAsync();

        var vehicle1 = new CustomerVehicle { CustomerId = customer1.CustomerId, ModelId = model.ModelId, LicensePlate = "30A-111", IsActive = true, CreatedDate = DateTime.UtcNow };
        var vehicle2 = new CustomerVehicle { CustomerId = customer1.CustomerId, ModelId = model.ModelId, LicensePlate = "30A-222", IsActive = true, CreatedDate = DateTime.UtcNow };
        var vehicle3 = new CustomerVehicle { CustomerId = customer2.CustomerId, ModelId = model.ModelId, LicensePlate = "30A-333", IsActive = true, CreatedDate = DateTime.UtcNow };
        DbContext.CustomerVehicles.AddRange(vehicle1, vehicle2, vehicle3);
        await DbContext.SaveChangesAsync();

        // Act
        var customer1Vehicles = await DbContext.CustomerVehicles
            .Where(v => v.CustomerId == customer1.CustomerId)
            .ToListAsync();

        // Assert
        customer1Vehicles.Should().HaveCount(2);
        customer1Vehicles.Should().Contain(v => v.LicensePlate == "30A-111");
        customer1Vehicles.Should().Contain(v => v.LicensePlate == "30A-222");
        customer1Vehicles.Should().NotContain(v => v.LicensePlate == "30A-333");
    }

    [Fact]
    public async Task CancelAppointment_ShouldUpdateStatus()
    {
        // Arrange
        var customer = new Customer { CustomerCode = "CUST001", FullName = "Test", Email = "test@test.com", PhoneNumber = "0901234567", IsActive = true, CreatedDate = DateTime.UtcNow };
        DbContext.Customers.Add(customer);

        var brand = new CarBrand { BrandName = "VinFast", IsActive = true };
        DbContext.CarBrands.Add(brand);
        await DbContext.SaveChangesAsync();

        var model = new CarModel { BrandId = brand.BrandId, ModelName = "VF 8", Year = 2024, IsActive = true };
        DbContext.CarModels.Add(model);
        await DbContext.SaveChangesAsync();

        var vehicle = new CustomerVehicle { CustomerId = customer.CustomerId, ModelId = model.ModelId, LicensePlate = "30A-123", IsActive = true, CreatedDate = DateTime.UtcNow };
        DbContext.CustomerVehicles.Add(vehicle);

        var serviceCenter = new ServiceCenter { CenterCode = "SC001", CenterName = "Center", Address = "123", PhoneNumber = "024123", IsActive = true, CreatedDate = DateTime.UtcNow };
        DbContext.ServiceCenters.Add(serviceCenter);
        await DbContext.SaveChangesAsync();

        var appointment = new Appointment
        {
            CustomerId = customer.CustomerId,
            VehicleId = vehicle.VehicleId,
            ServiceCenterId = serviceCenter.CenterId,
            AppointmentCode = "APT001",
            AppointmentDate = DateTime.UtcNow.AddDays(1),
            StatusId = 1, // Pending
            RowVersion = new byte[] { 0, 0, 0, 0, 0, 0, 0, 1 }, // Mock RowVersion
            CreatedDate = DateTime.UtcNow
        };
        DbContext.Appointments.Add(appointment);
        await DbContext.SaveChangesAsync();

        // Act
        appointment.StatusId = 4; // Cancelled
        appointment.CancellationReason = "Test cancellation";
        appointment.CancelledDate = DateTime.UtcNow;
        await DbContext.SaveChangesAsync();

        // Assert
        var saved = await DbContext.Appointments.FirstOrDefaultAsync(a => a.AppointmentCode == "APT001");
        saved.Should().NotBeNull();
        saved!.StatusId.Should().Be(4);
        saved.CancellationReason.Should().Be("Test cancellation");
        saved.CancelledDate.Should().NotBeNull();
    }
}
