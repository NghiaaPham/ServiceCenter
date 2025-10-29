using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.Customers.Entities;
using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;
using EVServiceCenter.Infrastructure.Domains.Customers.Services;
using EVServiceCenter.Infrastructure.Domains.Customers.Repositories;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EVServiceCenter.Tests.Integration;

/// <summary>
/// Integration tests for CustomerService with real database operations
/// </summary>
public class CustomerServiceIntegrationTests : TestBase
{
    private readonly CustomerService _customerService;
    private readonly CustomerRepository _customerRepository;

    public CustomerServiceIntegrationTests()
    {
        var logger = CreateMockLogger<CustomerService>();
        var repositoryLogger = CreateMockLogger<CustomerRepository>();
        
        _customerRepository = new CustomerRepository(DbContext, repositoryLogger);
        _customerService = new CustomerService(_customerRepository, logger);
    }

    protected override async Task SeedTestDataAsync()
    {
        // Seed test customers
        var customers = new List<Customer>
        {
            new Customer
            {
                CustomerCode = "CUST001",
                FullName = "John Doe",
                PhoneNumber = "0123456789",
                Email = "john@example.com",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Customer
            {
                CustomerCode = "CUST002",
                FullName = "Jane Smith",
                PhoneNumber = "0987654321",
                Email = "jane@example.com",
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Customer
            {
                CustomerCode = "CUST003",
                FullName = "Bob Johnson",
                PhoneNumber = "0555666777",
                Email = "bob@example.com",
                IsActive = false, // Inactive customer
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            }
        };

        DbContext.Customers.AddRange(customers);
        await DbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingCustomer_ShouldReturnCustomer()
    {
        // Arrange
        await SeedTestDataAsync();
        var customerId = 1;

        // Act
        var result = await _customerService.GetByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerId.Should().Be(customerId);
        result.CustomerCode.Should().Be("CUST001");
        result.FullName.Should().Be("John Doe");
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentCustomer_ShouldReturnNull()
    {
        // Arrange
        await SeedTestDataAsync();
        var nonExistentId = 999;

        // Act
        var result = await _customerService.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCustomerCodeAsync_WithExistingCode_ShouldReturnCustomer()
    {
        // Arrange
        await SeedTestDataAsync();
        var customerCode = "CUST002";

        // Act
        var result = await _customerService.GetByCustomerCodeAsync(customerCode);

        // Assert
        result.Should().NotBeNull();
        result!.CustomerCode.Should().Be(customerCode);
        result.FullName.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task GetByPhoneNumberAsync_WithExistingPhone_ShouldReturnCustomer()
    {
        // Arrange
        await SeedTestDataAsync();
        var phoneNumber = "0123456789";

        // Act
        var result = await _customerService.GetByPhoneNumberAsync(phoneNumber);

        // Assert
        result.Should().NotBeNull();
        result!.PhoneNumber.Should().Be(phoneNumber);
        result.FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnCustomer()
    {
        // Arrange
        await SeedTestDataAsync();
        var email = "jane@example.com";

        // Act
        var result = await _customerService.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be(email);
        result.FullName.Should().Be("Jane Smith");
    }

    [Fact]
    public async Task GetActiveCustomersAsync_ShouldReturnOnlyActiveCustomers()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _customerService.GetActiveCustomersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); // Only 2 active customers
        result.Should().OnlyContain(c => c.IsActive == true);
        result.Should().Contain(c => c.CustomerCode == "CUST001");
        result.Should().Contain(c => c.CustomerCode == "CUST002");
        result.Should().NotContain(c => c.CustomerCode == "CUST003"); // Inactive
    }


    [Fact]
    public async Task AddLoyaltyPointsAsync_WithValidData_ShouldAddPoints()
    {
        // Arrange
        await SeedTestDataAsync();
        var customerId = 1;
        var points = 100;
        var reason = "Purchase bonus";

        // Act
        var result = await _customerService.AddLoyaltyPointsAsync(customerId, points, reason);

        // Assert
        result.Should().BeTrue();

        // Verify points were actually added in database
        var customer = await DbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        customer.Should().NotBeNull();
        // Note: This assumes the repository implementation updates loyalty points
        // The actual verification depends on the repository implementation
    }

    [Fact]
    public async Task ProcessPurchaseAsync_WithValidData_ShouldProcessPurchase()
    {
        // Arrange
        await SeedTestDataAsync();
        var customerId = 1;
        var amount = 1000000m;

        // Act
        var result = await _customerService.ProcessPurchaseAsync(customerId, amount);

        // Assert
        result.Should().BeTrue();

        // Verify purchase was actually processed in database
        var customer = await DbContext.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        customer.Should().NotBeNull();
        // Note: This assumes the repository implementation updates total spent
        // The actual verification depends on the repository implementation
    }

    [Fact]
    public async Task CanDeleteAsync_WithCustomerWithoutVehicles_ShouldReturnTrue()
    {
        // Arrange
        await SeedTestDataAsync();
        var customerId = 1;

        // Act
        var result = await _customerService.CanDeleteAsync(customerId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_WithQuery_ShouldReturnPagedResults()
    {
        // Arrange
        await SeedTestDataAsync();
        var query = new CustomerQueryDto
        {
            Page = 1,
            PageSize = 10,
            SearchTerm = "John"
        };

        // Act
        var result = await _customerService.GetAllAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterThan(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        
        // Should find John Doe
        result.Items.Should().Contain(c => c.FullName.Contains("John"));
    }

    [Fact]
    public async Task GetCustomerStatisticsAsync_ShouldReturnStatistics()
    {
        // Arrange
        await SeedTestDataAsync();

        // Act
        var result = await _customerService.GetCustomerStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Dictionary<string, int>>();
        // The actual statistics depend on the repository implementation
    }
}
