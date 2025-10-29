using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenanceServices.DTOs.Responses;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Services;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.ServiceCategories.Entities;
using EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Repositories;
using EVServiceCenter.Infrastructure.Domains.MaintenanceServices.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace EVServiceCenter.Tests.Unit.Services
{
    public class MaintenanceServiceServiceTests
    {
        private readonly Mock<IMaintenanceServiceRepository> _mockRepository;
        private readonly Mock<IServiceCategoryRepository> _mockCategoryRepository;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<MaintenanceServiceService>> _mockLogger;
        private readonly MaintenanceServiceService _service;

        public MaintenanceServiceServiceTests()
        {
            _mockRepository = new Mock<IMaintenanceServiceRepository>();
            _mockCategoryRepository = new Mock<IServiceCategoryRepository>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<MaintenanceServiceService>>();
            
            _service = new MaintenanceServiceService(
                _mockRepository.Object,
                _mockCategoryRepository.Object,
                _mockCache.Object,
                _mockLogger.Object);
        }

        #region GetAllAsync Tests

       
        
        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnService()
        {
            // Arrange
            var serviceId = 1;
            var service = new MaintenanceService
            {
                ServiceId = serviceId,
                ServiceName = "Test Service",
                BasePrice = 100,
                Description = "Test Description",
                CategoryId = 1,
                IsActive = true
            };

            // Mock cache to return null (not cached)
            object? cachedValue = null;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            _mockRepository.Setup(x => x.GetByIdWithDetailsAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);

            // Mock cache Set method
            _mockCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            // Act
            var result = await _service.GetByIdAsync(serviceId);

            // Assert
            result.Should().NotBeNull();
            result.ServiceId.Should().Be(serviceId);
            result.ServiceName.Should().Be("Test Service");
            result.BasePrice.Should().Be(100);
            result.Description.Should().Be("Test Description");

            _mockRepository.Verify(x => x.GetByIdWithDetailsAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var serviceId = 999;

            // Mock cache to return null (not cached)
            object? cachedValue = null;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            _mockRepository.Setup(x => x.GetByIdWithDetailsAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MaintenanceService?)null);

            // Act
            var result = await _service.GetByIdAsync(serviceId);

            // Assert
            result.Should().BeNull();

            _mockRepository.Verify(x => x.GetByIdWithDetailsAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithInvalidCategoryId_ShouldThrowException()
        {
            // Arrange
            var request = new CreateMaintenanceServiceRequestDto
            {
                ServiceName = "New Service",
                CategoryId = 999
            };

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceCategory?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));

            _mockCategoryRepository.Verify(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.CreateAsync(It.IsAny<MaintenanceService>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateServiceCode_ShouldThrowException()
        {
            // Arrange
            var request = new CreateMaintenanceServiceRequestDto
            {
                ServiceName = "New Service",
                ServiceCode = "EXISTING_CODE",
                CategoryId = 1
            };

            var category = new ServiceCategory
            {
                CategoryId = 1,
                CategoryName = "Test Category"
            };

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _mockRepository.Setup(x => x.IsServiceCodeExistsAsync(request.ServiceCode, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));

            _mockCategoryRepository.Verify(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.IsServiceCodeExistsAsync(request.ServiceCode, It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.CreateAsync(It.IsAny<MaintenanceService>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithInvalidServiceId_ShouldThrowException()
        {
            // Arrange
            var request = new UpdateMaintenanceServiceRequestDto
            {
                ServiceId = 999,
                ServiceName = "Updated Service",
                CategoryId = 1
            };

            _mockRepository.Setup(x => x.GetByIdAsync(request.ServiceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MaintenanceService?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(request));

            _mockRepository.Verify(x => x.GetByIdAsync(request.ServiceId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<MaintenanceService>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidCategoryId_ShouldThrowException()
        {
            // Arrange
            var request = new UpdateMaintenanceServiceRequestDto
            {
                ServiceId = 1,
                ServiceName = "Updated Service",
                CategoryId = 999
            };

            var existingService = new MaintenanceService
            {
                ServiceId = 1,
                ServiceName = "Old Service",
                CategoryId = 1
            };

            _mockRepository.Setup(x => x.GetByIdAsync(request.ServiceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingService);

            _mockCategoryRepository.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceCategory?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(request));

            _mockRepository.Verify(x => x.GetByIdAsync(request.ServiceId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCategoryRepository.Verify(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<MaintenanceService>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteService()
        {
            // Arrange
            var serviceId = 1;
            var service = new MaintenanceService
            {
                ServiceId = serviceId,
                ServiceName = "Test Service",
                IsActive = true
            };

            _mockRepository.Setup(x => x.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);

            _mockRepository.Setup(x => x.CanDeleteAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockRepository.Setup(x => x.DeleteAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(serviceId);

            // Assert
            result.Should().BeTrue();

            _mockRepository.Verify(x => x.CanDeleteAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.DeleteAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        }

      
        [Fact]
        public async Task DeleteAsync_WhenCannotDelete_ShouldReturnFalse()
        {
            // Arrange
            var serviceId = 1;
            var service = new MaintenanceService
            {
                ServiceId = serviceId,
                ServiceName = "Test Service",
                IsActive = true
            };

            _mockRepository.Setup(x => x.GetByIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);

            _mockRepository.Setup(x => x.CanDeleteAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(serviceId));

            _mockRepository.Verify(x => x.CanDeleteAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.DeleteAsync(serviceId, It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region GetActiveServicesAsync Tests

        [Fact]
        public async Task GetActiveServicesAsync_ShouldReturnActiveServices()
        {
            // Arrange
            var services = new List<MaintenanceService>
            {
                new MaintenanceService { ServiceId = 1, ServiceName = "Active Service 1", IsActive = true },
                new MaintenanceService { ServiceId = 2, ServiceName = "Active Service 2", IsActive = true }
            };

            // Mock cache to return null (not cached)
            object? cachedValue = null;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            _mockRepository.Setup(x => x.GetActiveServicesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(services);

            // Mock cache Set method
            _mockCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            // Act
            var result = await _service.GetActiveServicesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(s => s.IsActive == true).Should().BeTrue();

            _mockRepository.Verify(x => x.GetActiveServicesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetServicesByCategoryAsync Tests

        [Fact]
        public async Task GetServicesByCategoryAsync_WithValidCategoryId_ShouldReturnServices()
        {
            // Arrange
            var categoryId = 1;
            var services = new List<MaintenanceService>
            {
                new MaintenanceService { ServiceId = 1, ServiceName = "Service 1", CategoryId = categoryId },
                new MaintenanceService { ServiceId = 2, ServiceName = "Service 2", CategoryId = categoryId }
            };

            _mockRepository.Setup(x => x.GetServicesByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(services);

            // Act
            var result = await _service.GetServicesByCategoryAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(s => s.CategoryId == categoryId).Should().BeTrue();

            _mockRepository.Verify(x => x.GetServicesByCategoryAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetServicesByCategoryAsync_WithInvalidCategoryId_ShouldReturnEmptyList()
        {
            // Arrange
            var categoryId = 999;

            _mockRepository.Setup(x => x.GetServicesByCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MaintenanceService>());

            // Act
            var result = await _service.GetServicesByCategoryAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();

            _mockRepository.Verify(x => x.GetServicesByCategoryAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region CanDeleteAsync Tests

        [Fact]
        public async Task CanDeleteAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var serviceId = 1;

            _mockRepository.Setup(x => x.CanDeleteAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CanDeleteAsync(serviceId);

            // Assert
            result.Should().BeTrue();

            _mockRepository.Verify(x => x.CanDeleteAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CanDeleteAsync_WhenCannotDelete_ShouldReturnFalse()
        {
            // Arrange
            var serviceId = 1;

            _mockRepository.Setup(x => x.CanDeleteAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CanDeleteAsync(serviceId);

            // Assert
            result.Should().BeFalse();

            _mockRepository.Verify(x => x.CanDeleteAsync(serviceId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion
    }
}