using EVServiceCenter.Core.Entities;
using EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Requests;
using EVServiceCenter.Core.Domains.ModelServicePricings.DTOs.Responses;
using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Services;
using EVServiceCenter.Core.Domains.ModelServicePricings.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.CarModels.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenanceServices.Interfaces.Repositories;
using EVServiceCenter.Infrastructure.Domains.ModelServicePricings.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Domains.CarModels.Entities;
using EVServiceCenter.Core.Domains.MaintenanceServices.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EVServiceCenter.Tests.Unit.Services
{
    public class ModelServicePricingServiceTests
    {
        private readonly Mock<IModelServicePricingRepository> _mockRepository;
        private readonly Mock<ICarModelRepository> _mockModelRepository;
        private readonly Mock<IMaintenanceServiceRepository> _mockServiceRepository;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<ModelServicePricingService>> _mockLogger;
        private readonly ModelServicePricingService _service;

        public ModelServicePricingServiceTests()
        {
            _mockRepository = new Mock<IModelServicePricingRepository>();
            _mockModelRepository = new Mock<ICarModelRepository>();
            _mockServiceRepository = new Mock<IMaintenanceServiceRepository>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<ModelServicePricingService>>();
            
            _service = new ModelServicePricingService(
                _mockRepository.Object,
                _mockModelRepository.Object,
                _mockServiceRepository.Object,
                _mockCache.Object,
                _mockLogger.Object);
        }

        #region GetAllAsync Tests

       
      
        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var pricingId = 999;

            // Mock cache to return null (not cached)
            object? cachedValue = null;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            // Mock repository to return null
            _mockRepository.Setup(x => x.GetByIdAsync(pricingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ModelServicePricing?)null);

            // Act
            var result = await _service.GetByIdAsync(pricingId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region CreateAsync Tests

    
        [Fact]
        public async Task CreateAsync_WithInvalidModelId_ShouldThrowException()
        {
            // Arrange
            var request = new CreateModelServicePricingRequestDto
            {
                ModelId = 999,
                ServiceId = 1,
                CustomPrice = 100000,
                IsActive = true
            };

            // Mock repository to return null for model
            _mockModelRepository.Setup(x => x.GetByIdAsync(request.ModelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CarModel?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_WithInvalidServiceId_ShouldThrowException()
        {
            // Arrange
            var request = new CreateModelServicePricingRequestDto
            {
                ModelId = 1,
                ServiceId = 999,
                CustomPrice = 100000,
                IsActive = true
            };

            var model = new CarModel { ModelId = 1, ModelName = "Test Model" };

            // Mock repository methods
            _mockModelRepository.Setup(x => x.GetByIdAsync(request.ModelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(model);
            _mockServiceRepository.Setup(x => x.GetByIdAsync(request.ServiceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MaintenanceService?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));
        }

        [Fact]
        public async Task CreateAsync_WithDuplicatePricing_ShouldThrowException()
        {
            // Arrange
            var request = new CreateModelServicePricingRequestDto
            {
                ModelId = 1,
                ServiceId = 1,
                CustomPrice = 100000,
                IsActive = true
            };

            var model = new CarModel { ModelId = 1, ModelName = "Test Model" };
            var service = new MaintenanceService { ServiceId = 1, ServiceName = "Test Service" };
            var existingPricing = new ModelServicePricing { PricingId = 1, ModelId = 1, ServiceId = 1 };

            // Mock repository methods
            _mockModelRepository.Setup(x => x.GetByIdAsync(request.ModelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(model);
            _mockServiceRepository.Setup(x => x.GetByIdAsync(request.ServiceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(service);
            _mockRepository.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<ModelServicePricing, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPricing);

            // Act & Assert
            await Assert.ThrowsAsync<NullReferenceException>(() => _service.CreateAsync(request));
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithInvalidPricingId_ShouldThrowException()
        {
            // Arrange
            var request = new UpdateModelServicePricingRequestDto
            {
                PricingId = 999,
                ModelId = 1,
                ServiceId = 1,
                CustomPrice = 150000,
                IsActive = true
            };

            // Mock repository to return null
            _mockRepository.Setup(x => x.GetByIdAsync(request.PricingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ModelServicePricing?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(request));
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeletePricing()
        {
            // Arrange
            var pricingId = 1;
            var existingPricing = new ModelServicePricing
            {
                PricingId = pricingId,
                ModelId = 1,
                ServiceId = 1,
                CustomPrice = 100000,
                IsActive = true
            };

            // Mock repository methods
            _mockRepository.Setup(x => x.GetByIdAsync(pricingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingPricing);
            _mockRepository.Setup(x => x.DeleteAsync(pricingId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(pricingId);

            // Assert
            result.Should().BeTrue();
            _mockRepository.Verify(x => x.DeleteAsync(pricingId, It.IsAny<CancellationToken>()), Times.Once);
        }


        #endregion

        #region GetByModelIdAsync Tests

        [Fact]
        public async Task GetByModelIdAsync_WithValidModelId_ShouldReturnPricings()
        {
            // Arrange
            var modelId = 1;
            var pricings = new List<ModelServicePricing>
            {
                new ModelServicePricing
                {
                    PricingId = 1,
                    ModelId = modelId,
                    ServiceId = 1,
                    CustomPrice = 100000,
                    IsActive = true,
                    Model = new CarModel { ModelId = 1, ModelName = "Test Model" },
                    Service = new MaintenanceService { ServiceId = 1, ServiceName = "Test Service" }
                }
            };

            // Mock repository
            _mockRepository.Setup(x => x.GetByModelIdAsync(modelId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pricings);

            // Act
            var result = await _service.GetByModelIdAsync(modelId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().ModelId.Should().Be(modelId);
        }

        #endregion

        #region GetByServiceIdAsync Tests

        [Fact]
        public async Task GetByServiceIdAsync_WithValidServiceId_ShouldReturnPricings()
        {
            // Arrange
            var serviceId = 1;
            var pricings = new List<ModelServicePricing>
            {
                new ModelServicePricing
                {
                    PricingId = 1,
                    ModelId = 1,
                    ServiceId = serviceId,
                    CustomPrice = 100000,
                    IsActive = true,
                    Model = new CarModel { ModelId = 1, ModelName = "Test Model" },
                    Service = new MaintenanceService { ServiceId = 1, ServiceName = "Test Service" }
                }
            };

            // Mock repository
            _mockRepository.Setup(x => x.GetByServiceIdAsync(serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pricings);

            // Act
            var result = await _service.GetByServiceIdAsync(serviceId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.First().ServiceId.Should().Be(serviceId);
        }

        #endregion

        #region GetActivePricingAsync Tests

        [Fact]
        public async Task GetActivePricingAsync_WithValidParameters_ShouldReturnActivePricing()
        {
            // Arrange
            var modelId = 1;
            var serviceId = 1;
            var activePricing = new ModelServicePricing
            {
                PricingId = 1,
                ModelId = modelId,
                ServiceId = serviceId,
                CustomPrice = 100000,
                IsActive = true,
                Model = new CarModel { ModelId = 1, ModelName = "Test Model" },
                Service = new MaintenanceService { ServiceId = 1, ServiceName = "Test Service" }
            };

            // Mock repository
            _mockRepository.Setup(x => x.GetActivePricingAsync(modelId, serviceId, It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(activePricing);

            // Act
            var result = await _service.GetActivePricingAsync(modelId, serviceId);

            // Assert
            result.Should().NotBeNull();
            result.PricingId.Should().Be(1);
            result.IsActive.Should().BeTrue();
        }

        [Fact]
        public async Task GetActivePricingAsync_WithNoActivePricing_ShouldReturnNull()
        {
            // Arrange
            var modelId = 1;
            var serviceId = 1;

            // Mock repository to return null
            _mockRepository.Setup(x => x.GetActivePricingAsync(modelId, serviceId, It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ModelServicePricing?)null);

            // Act
            var result = await _service.GetActivePricingAsync(modelId, serviceId);

            // Assert
            result.Should().BeNull();
        }

        #endregion
    }
}