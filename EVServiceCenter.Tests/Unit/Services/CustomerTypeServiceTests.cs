using EVServiceCenter.Core.Domains.CustomerTypes.Entities;
using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Requests;
using EVServiceCenter.Core.Domains.CustomerTypes.DTOs.Responses;
using EVServiceCenter.Core.Domains.CustomerTypes.Interfaces.Repositories;
using EVServiceCenter.Infrastructure.Domains.CustomerTypes.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace EVServiceCenter.Tests.Unit.Services
{
    public class CustomerTypeServiceTests
    {
        private readonly Mock<ICustomerTypeRepository> _mockRepository;
        private readonly Mock<ILogger<CustomerTypeService>> _mockLogger;
        private readonly CustomerTypeService _service;

        public CustomerTypeServiceTests()
        {
            _mockRepository = new Mock<ICustomerTypeRepository>();
            _mockLogger = new Mock<ILogger<CustomerTypeService>>();
            
            _service = new CustomerTypeService(
                _mockRepository.Object,
                _mockLogger.Object);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithValidQuery_ShouldReturnCustomerTypes()
        {
            // Arrange
            var query = new CustomerTypeQueryDto
            {
                Page = 1,
                PageSize = 10,
                SearchTerm = "VIP",
                IsActive = true,
                SortBy = "TypeName",
                SortDesc = false,
                IncludeStats = true
            };

            var pagedResult = new PagedResult<CustomerTypeResponseDto>
            {
                Items = new List<CustomerTypeResponseDto>
                {
                    new CustomerTypeResponseDto { TypeId = 1, TypeName = "VIP Customer", DiscountPercent = 10 },
                    new CustomerTypeResponseDto { TypeId = 2, TypeName = "Premium Customer", DiscountPercent = 15 }
                },
                TotalCount = 2,
                Page = 1,
                PageSize = 10
            };

            _mockRepository.Setup(x => x.GetAllAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _service.GetAllAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
            result.Page.Should().Be(1);
            result.PageSize.Should().Be(10);

            _mockRepository.Verify(x => x.GetAllAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyQuery_ShouldReturnAllCustomerTypes()
        {
            // Arrange
            var query = new CustomerTypeQueryDto();

            var pagedResult = new PagedResult<CustomerTypeResponseDto>
            {
                Items = new List<CustomerTypeResponseDto>
                {
                    new CustomerTypeResponseDto { TypeId = 1, TypeName = "Standard Customer", DiscountPercent = 0 }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 10
            };

            _mockRepository.Setup(x => x.GetAllAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _service.GetAllAsync(query);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);

            _mockRepository.Verify(x => x.GetAllAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnCustomerType()
        {
            // Arrange
            var typeId = 1;
            var customerType = new CustomerTypeResponseDto
            {
                TypeId = typeId,
                TypeName = "VIP Customer",
                DiscountPercent = 10,
                Description = "VIP Customer Type",
                IsActive = true,
                CustomerCount = 5,
                ActiveCustomerCount = 4,
                TotalRevenueFromType = 10000
            };

            _mockRepository.Setup(x => x.GetByIdAsync(typeId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customerType);

            // Act
            var result = await _service.GetByIdAsync(typeId);

            // Assert
            result.Should().NotBeNull();
            result.TypeId.Should().Be(typeId);
            result.TypeName.Should().Be("VIP Customer");
            result.DiscountPercent.Should().Be(10);
            result.Description.Should().Be("VIP Customer Type");
            result.IsActive.Should().BeTrue();
            result.CustomerCount.Should().Be(5);
            result.ActiveCustomerCount.Should().Be(4);
            result.TotalRevenueFromType.Should().Be(10000);

            _mockRepository.Verify(x => x.GetByIdAsync(typeId, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            var typeId = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.GetByIdAsync(typeId));

            _mockRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            var typeId = 999;

            _mockRepository.Setup(x => x.GetByIdAsync(typeId, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CustomerTypeResponseDto?)null);

            // Act
            var result = await _service.GetByIdAsync(typeId);

            // Assert
            result.Should().BeNull();

            _mockRepository.Verify(x => x.GetByIdAsync(typeId, true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithIncludeStatsFalse_ShouldCallRepositoryWithCorrectParameter()
        {
            // Arrange
            var typeId = 1;
            var customerType = new CustomerTypeResponseDto
            {
                TypeId = typeId,
                TypeName = "VIP Customer",
                DiscountPercent = 10,
                IsActive = true
            };

            _mockRepository.Setup(x => x.GetByIdAsync(typeId, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(customerType);

            // Act
            var result = await _service.GetByIdAsync(typeId, false);

            // Assert
            result.Should().NotBeNull();
            result.TypeId.Should().Be(typeId);

            _mockRepository.Verify(x => x.GetByIdAsync(typeId, false, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldCreateCustomerType()
        {
            // Arrange
            var request = new CreateCustomerTypeRequestDto
            {
                TypeName = "New Customer Type",
                DiscountPercent = 5,
                Description = "New Customer Type Description",
                IsActive = true
            };

            var createdCustomerType = new CustomerTypeResponseDto
            {
                TypeId = 1,
                TypeName = request.TypeName,
                DiscountPercent = request.DiscountPercent,
                Description = request.Description,
                IsActive = request.IsActive
            };

            _mockRepository.Setup(x => x.CreateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdCustomerType);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.TypeId.Should().Be(1);
            result.TypeName.Should().Be(request.TypeName);
            result.DiscountPercent.Should().Be(request.DiscountPercent);
            result.Description.Should().Be(request.Description);
            result.IsActive.Should().Be(request.IsActive);

            _mockRepository.Verify(x => x.CreateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldUpdateCustomerType()
        {
            // Arrange
            var request = new UpdateCustomerTypeRequestDto
            {
                TypeId = 1,
                TypeName = "Updated Customer Type",
                DiscountPercent = 15,
                Description = "Updated Description",
                IsActive = true
            };

            var updatedCustomerType = new CustomerTypeResponseDto
            {
                TypeId = 1,
                TypeName = request.TypeName,
                DiscountPercent = request.DiscountPercent,
                Description = request.Description,
                IsActive = request.IsActive
            };

            _mockRepository.Setup(x => x.UpdateAsync(request, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedCustomerType);

            // Act
            var result = await _service.UpdateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.TypeId.Should().Be(1);
            result.TypeName.Should().Be(request.TypeName);
            result.DiscountPercent.Should().Be(request.DiscountPercent);
            result.Description.Should().Be(request.Description);
            result.IsActive.Should().Be(request.IsActive);

            _mockRepository.Verify(x => x.UpdateAsync(request, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteCustomerType()
        {
            // Arrange
            var typeId = 1;

            _mockRepository.Setup(x => x.HasCustomersAsync(typeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(x => x.DeleteAsync(typeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(typeId);

            // Assert
            result.Should().BeTrue();

            _mockRepository.Verify(x => x.HasCustomersAsync(typeId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.DeleteAsync(typeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            var typeId = 0;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteAsync(typeId));

            _mockRepository.Verify(x => x.HasCustomersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockRepository.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenHasCustomers_ShouldThrowException()
        {
            // Arrange
            var typeId = 1;

            _mockRepository.Setup(x => x.HasCustomersAsync(typeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(typeId));

            _mockRepository.Verify(x => x.HasCustomersAsync(typeId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.DeleteAsync(typeId, It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region GetActiveTypesAsync Tests

        [Fact]
        public async Task GetActiveTypesAsync_ShouldReturnActiveCustomerTypes()
        {
            // Arrange
            var activeTypes = new List<CustomerTypeResponseDto>
            {
                new CustomerTypeResponseDto { TypeId = 1, TypeName = "Active Type 1", IsActive = true },
                new CustomerTypeResponseDto { TypeId = 2, TypeName = "Active Type 2", IsActive = true }
            };

            _mockRepository.Setup(x => x.GetActiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeTypes);

            // Act
            var result = await _service.GetActiveTypesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(t => t.IsActive == true).Should().BeTrue();

            _mockRepository.Verify(x => x.GetActiveAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region CanDeleteAsync Tests

        [Fact]
        public async Task CanDeleteAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var typeId = 1;

            _mockRepository.Setup(x => x.HasCustomersAsync(typeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CanDeleteAsync(typeId);

            // Assert
            result.Should().BeTrue();

            _mockRepository.Verify(x => x.HasCustomersAsync(typeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CanDeleteAsync_WhenHasCustomers_ShouldReturnFalse()
        {
            // Arrange
            var typeId = 1;

            _mockRepository.Setup(x => x.HasCustomersAsync(typeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CanDeleteAsync(typeId);

            // Assert
            result.Should().BeFalse();

            _mockRepository.Verify(x => x.HasCustomersAsync(typeId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion
    }
}
