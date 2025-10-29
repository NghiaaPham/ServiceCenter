using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using EVServiceCenter.Infrastructure.Domains.Customers.Services;
using EVServiceCenter.Core.Domains.Customers.Interfaces;
using EVServiceCenter.Core.Domains.Customers.DTOs.Requests;
using EVServiceCenter.Core.Domains.Customers.DTOs.Responses;
using EVServiceCenter.Core.Domains.Shared.Models;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Services;

/// <summary>
/// Unit tests for CustomerService
/// </summary>
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _mockRepository;
    private readonly Mock<ILogger<CustomerService>> _mockLogger;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _mockRepository = new Mock<ICustomerRepository>();
        _mockLogger = new Mock<ILogger<CustomerService>>();
        _service = new CustomerService(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnCustomer()
    {
        // Arrange
        var customerId = 1;
        var expectedCustomer = new CustomerResponseDto
        {
            CustomerId = customerId,
            CustomerCode = "CUST001",
            FullName = "John Doe"
        };

        _mockRepository
            .Setup(x => x.GetByIdAsync(customerId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _service.GetByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedCustomer);
        _mockRepository.Verify(x => x.GetByIdAsync(customerId, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(invalidId));
        _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByCustomerCodeAsync_WithValidCode_ShouldReturnCustomer()
    {
        // Arrange
        var customerCode = "CUST001";
        var expectedCustomer = new CustomerResponseDto
        {
            CustomerId = 1,
            CustomerCode = customerCode,
            FullName = "John Doe"
        };

        _mockRepository
            .Setup(x => x.GetByCustomerCodeAsync(customerCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _service.GetByCustomerCodeAsync(customerCode);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedCustomer);
        _mockRepository.Verify(x => x.GetByCustomerCodeAsync(customerCode, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetByCustomerCodeAsync_WithInvalidCode_ShouldThrowArgumentException(string customerCode)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByCustomerCodeAsync(customerCode));
        _mockRepository.Verify(x => x.GetByCustomerCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByPhoneNumberAsync_WithValidPhone_ShouldReturnCustomer()
    {
        // Arrange
        var phoneNumber = "0123456789";
        var expectedCustomer = new CustomerResponseDto
        {
            CustomerId = 1,
            PhoneNumber = phoneNumber,
            FullName = "John Doe"
        };

        _mockRepository
            .Setup(x => x.GetByPhoneNumberAsync(phoneNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _service.GetByPhoneNumberAsync(phoneNumber);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedCustomer);
        _mockRepository.Verify(x => x.GetByPhoneNumberAsync(phoneNumber, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetByPhoneNumberAsync_WithInvalidPhone_ShouldThrowArgumentException(string phoneNumber)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByPhoneNumberAsync(phoneNumber));
        _mockRepository.Verify(x => x.GetByPhoneNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ShouldReturnCustomer()
    {
        // Arrange
        var email = "john@example.com";
        var expectedCustomer = new CustomerResponseDto
        {
            CustomerId = 1,
            Email = email,
            FullName = "John Doe"
        };

        _mockRepository
            .Setup(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _service.GetByEmailAsync(email);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedCustomer);
        _mockRepository.Verify(x => x.GetByEmailAsync(email, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetByEmailAsync_WithInvalidEmail_ShouldThrowArgumentException(string email)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByEmailAsync(email));
        _mockRepository.Verify(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WithValidRequest_ShouldReturnUpdatedCustomer()
    {
        // Arrange
        var request = new UpdateCustomerRequestDto
        {
            CustomerId = 1,
            FullName = "John Updated",
            PhoneNumber = "0987654321"
        };

        var expectedResult = new CustomerResponseDto
        {
            CustomerId = 1,
            CustomerCode = "CUST001",
            FullName = "John Updated",
            PhoneNumber = "0987654321"
        };

        _mockRepository
            .Setup(x => x.UpdateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.UpdateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);
        _mockRepository.Verify(x => x.UpdateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidIdAndCanDelete_ShouldReturnTrue()
    {
        // Arrange
        var customerId = 1;

        _mockRepository
            .Setup(x => x.HasVehiclesAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Can delete

        _mockRepository
            .Setup(x => x.DeleteAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(customerId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.HasVehiclesAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidIdButCannotDelete_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customerId = 1;

        _mockRepository
            .Setup(x => x.HasVehiclesAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Cannot delete - has vehicles

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(customerId));
        _mockRepository.Verify(x => x.HasVehiclesAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidId_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidId = 0;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(invalidId));
        _mockRepository.Verify(x => x.HasVehiclesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CanDeleteAsync_WithCustomerWithoutVehicles_ShouldReturnTrue()
    {
        // Arrange
        var customerId = 1;

        _mockRepository
            .Setup(x => x.HasVehiclesAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CanDeleteAsync(customerId);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.HasVehiclesAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CanDeleteAsync_WithCustomerWithVehicles_ShouldReturnFalse()
    {
        // Arrange
        var customerId = 1;

        _mockRepository
            .Setup(x => x.HasVehiclesAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CanDeleteAsync(customerId);

        // Assert
        result.Should().BeFalse();
        _mockRepository.Verify(x => x.HasVehiclesAsync(customerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddLoyaltyPointsAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var customerId = 1;
        var points = 100;
        var reason = "Purchase bonus";

        _mockRepository
            .Setup(x => x.UpdateLoyaltyPointsAsync(customerId, points, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.AddLoyaltyPointsAsync(customerId, points, reason);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.UpdateLoyaltyPointsAsync(customerId, points, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 100, "reason")]
    [InlineData(-1, 100, "reason")]
    [InlineData(1, 0, "reason")]
    [InlineData(1, -1, "reason")]
    public async Task AddLoyaltyPointsAsync_WithInvalidData_ShouldThrowArgumentException(int customerId, int points, string reason)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.AddLoyaltyPointsAsync(customerId, points, reason));
        _mockRepository.Verify(x => x.UpdateLoyaltyPointsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessPurchaseAsync_WithValidData_ShouldReturnTrue()
    {
        // Arrange
        var customerId = 1;
        var amount = 1000000m;

        _mockRepository
            .Setup(x => x.UpdateTotalSpentAsync(customerId, amount, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.ProcessPurchaseAsync(customerId, amount);

        // Assert
        result.Should().BeTrue();
        _mockRepository.Verify(x => x.UpdateTotalSpentAsync(customerId, amount, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(0, 1000000)]
    [InlineData(-1, 1000000)]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public async Task ProcessPurchaseAsync_WithInvalidData_ShouldThrowArgumentException(int customerId, decimal amount)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessPurchaseAsync(customerId, amount));
        _mockRepository.Verify(x => x.UpdateTotalSpentAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateWalkInCustomerAsync_WithValidRequest_ShouldReturnCustomer()
    {
        // Arrange
        var request = new CreateWalkInCustomerDto
        {
            FullName = "Walk-in Customer",
            PhoneNumber = "0123456789",
            Email = "walkin@example.com"
        };

        var createdByUserId = 1;

        var expectedResult = new CustomerResponseDto
        {
            CustomerId = 1,
            CustomerCode = "CUST001",
            FullName = "Walk-in Customer",
            PhoneNumber = "0123456789",
            Email = "walkin@example.com"
        };

        _mockRepository
            .Setup(x => x.CreateAsync(It.IsAny<CreateCustomerRequestDto>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.CreateWalkInCustomerAsync(request, createdByUserId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);
        _mockRepository.Verify(x => x.CreateAsync(It.IsAny<CreateCustomerRequestDto>(), null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_WithQuery_ShouldReturnPagedResult()
    {
        // Arrange
        var query = new CustomerQueryDto
        {
            Page = 1,
            PageSize = 10
        };

        var expectedResult = new PagedResult<CustomerResponseDto>
        {
            Items = new List<CustomerResponseDto>
            {
                new CustomerResponseDto { CustomerId = 1, FullName = "John Doe" },
                new CustomerResponseDto { CustomerId = 2, FullName = "Jane Smith" }
            },
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };

        _mockRepository
            .Setup(x => x.GetAllAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.GetAllAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResult);
        _mockRepository.Verify(x => x.GetAllAsync(query, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetActiveCustomersAsync_ShouldReturnCustomers()
    {
        // Arrange
        var expectedCustomers = new List<CustomerResponseDto>
        {
            new CustomerResponseDto { CustomerId = 1, FullName = "John Doe", IsActive = true },
            new CustomerResponseDto { CustomerId = 2, FullName = "Jane Smith", IsActive = true }
        };

        _mockRepository
            .Setup(x => x.GetActiveCustomersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCustomers);

        // Act
        var result = await _service.GetActiveCustomersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedCustomers);
        _mockRepository.Verify(x => x.GetActiveCustomersAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomersWithMaintenanceDueAsync_ShouldReturnCustomers()
    {
        // Arrange
        var expectedCustomers = new List<CustomerResponseDto>
        {
            new CustomerResponseDto { CustomerId = 1, FullName = "John Doe" }
        };

        _mockRepository
            .Setup(x => x.GetCustomersWithMaintenanceDueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedCustomers);

        // Act
        var result = await _service.GetCustomersWithMaintenanceDueAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedCustomers);
        _mockRepository.Verify(x => x.GetCustomersWithMaintenanceDueAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCustomerStatisticsAsync_ShouldReturnStatistics()
    {
        // Arrange
        var expectedStats = new Dictionary<string, int>
        {
            { "VIP", 5 },
            { "Regular", 10 },
            { "Premium", 3 }
        };

        _mockRepository
            .Setup(x => x.GetCustomerStatsByTypeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _service.GetCustomerStatisticsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedStats);
        _mockRepository.Verify(x => x.GetCustomerStatsByTypeAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
