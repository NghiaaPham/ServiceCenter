using EVServiceCenter.Core.Domains.ServiceCategories.Entities;
using EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Requests;
using EVServiceCenter.Core.Domains.ServiceCategories.DTOs.Responses;
using EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Services;
using EVServiceCenter.Core.Domains.ServiceCategories.Interfaces.Repositories;
using EVServiceCenter.Infrastructure.Domains.ServiceCategories.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace EVServiceCenter.Tests.Unit.Services
{
    public class ServiceCategoryServiceTests
    {
        private readonly Mock<IServiceCategoryRepository> _mockRepository;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly Mock<ILogger<ServiceCategoryService>> _mockLogger;
        private readonly ServiceCategoryService _service;

        public ServiceCategoryServiceTests()
        {
            _mockRepository = new Mock<IServiceCategoryRepository>();
            _mockCache = new Mock<IMemoryCache>();
            _mockLogger = new Mock<ILogger<ServiceCategoryService>>();
            
            _service = new ServiceCategoryService(
                _mockRepository.Object,
                _mockCache.Object,
                _mockLogger.Object);
        }

  

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new ServiceCategory
            {
                CategoryId = categoryId,
                CategoryName = "Test Category",
                Description = "Test Description",
                DisplayOrder = 1,
                IsActive = true
            };

            // Mock cache to return null (not cached)
            object? cachedValue = null;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            _mockRepository.Setup(x => x.GetByIdWithDetailsAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // Mock cache Set method
            _mockCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            // Act
            var result = await _service.GetByIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().Be(categoryId);
            result.CategoryName.Should().Be("Test Category");
            result.Description.Should().Be("Test Description");
            result.DisplayOrder.Should().Be(1);
            result.IsActive.Should().BeTrue();

            _mockRepository.Verify(x => x.GetByIdWithDetailsAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Arrange
            var categoryId = 999;

            // Mock cache to return null (not cached)
            object? cachedValue = null;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            _mockRepository.Setup(x => x.GetByIdWithDetailsAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceCategory?)null);

            // Act
            var result = await _service.GetByIdAsync(categoryId);

            // Assert
            result.Should().BeNull();

            _mockRepository.Verify(x => x.GetByIdWithDetailsAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldCreateCategory()
        {
            // Arrange
            var request = new CreateServiceCategoryRequestDto
            {
                CategoryName = "New Category",
                Description = "New Description",
                IconUrl = "icon.png",
                DisplayOrder = 1,
                IsActive = true
            };

            var createdCategory = new ServiceCategory
            {
                CategoryId = 1,
                CategoryName = request.CategoryName,
                Description = request.Description,
                IconUrl = request.IconUrl,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive
            };

            _mockRepository.Setup(x => x.IsCategoryNameExistsAsync(request.CategoryName, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(x => x.CreateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdCategory);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().Be(1);
            result.CategoryName.Should().Be(request.CategoryName);
            result.Description.Should().Be(request.Description);
            result.IconUrl.Should().Be(request.IconUrl);
            result.DisplayOrder.Should().Be(request.DisplayOrder);
            result.IsActive.Should().Be(request.IsActive);

            _mockRepository.Verify(x => x.IsCategoryNameExistsAsync(request.CategoryName, It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.CreateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithDuplicateCategoryName_ShouldThrowException()
        {
            // Arrange
            var request = new CreateServiceCategoryRequestDto
            {
                CategoryName = "Existing Category"
            };

            _mockRepository.Setup(x => x.IsCategoryNameExistsAsync(request.CategoryName, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));

            _mockRepository.Verify(x => x.IsCategoryNameExistsAsync(request.CategoryName, It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.CreateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldUpdateCategory()
        {
            // Arrange
            var request = new UpdateServiceCategoryRequestDto
            {
                CategoryId = 1,
                CategoryName = "Updated Category",
                Description = "Updated Description",
                IconUrl = "updated-icon.png",
                DisplayOrder = 2,
                IsActive = true
            };

            var existingCategory = new ServiceCategory
            {
                CategoryId = 1,
                CategoryName = "Old Category",
                Description = "Old Description",
                IconUrl = "old-icon.png",
                DisplayOrder = 1,
                IsActive = true
            };

            var updatedCategory = new ServiceCategory
            {
                CategoryId = 1,
                CategoryName = request.CategoryName,
                Description = request.Description,
                IconUrl = request.IconUrl,
                DisplayOrder = request.DisplayOrder,
                IsActive = request.IsActive
            };

            _mockRepository.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCategory);

            _mockRepository.Setup(x => x.IsCategoryNameExistsAsync(request.CategoryName, request.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedCategory);

            // Act
            var result = await _service.UpdateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.CategoryId.Should().Be(1);
            result.CategoryName.Should().Be(request.CategoryName);
            result.Description.Should().Be(request.Description);
            result.IconUrl.Should().Be(request.IconUrl);
            result.DisplayOrder.Should().Be(request.DisplayOrder);
            result.IsActive.Should().Be(request.IsActive);

            _mockRepository.Verify(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.IsCategoryNameExistsAsync(request.CategoryName, request.CategoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidCategoryId_ShouldThrowException()
        {
            // Arrange
            var request = new UpdateServiceCategoryRequestDto
            {
                CategoryId = 999,
                CategoryName = "Updated Category"
            };

            _mockRepository.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceCategory?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(request));

            _mockRepository.Verify(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithDuplicateCategoryName_ShouldThrowException()
        {
            // Arrange
            var request = new UpdateServiceCategoryRequestDto
            {
                CategoryId = 1,
                CategoryName = "Existing Category"
            };

            var existingCategory = new ServiceCategory
            {
                CategoryId = 1,
                CategoryName = "Old Category"
            };

            _mockRepository.Setup(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingCategory);

            _mockRepository.Setup(x => x.IsCategoryNameExistsAsync(request.CategoryName, request.CategoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(request));

            _mockRepository.Verify(x => x.GetByIdAsync(request.CategoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.IsCategoryNameExistsAsync(request.CategoryName, request.CategoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<ServiceCategory>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new ServiceCategory
            {
                CategoryId = categoryId,
                CategoryName = "Test Category",
                IsActive = true
            };

            _mockRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _mockRepository.Setup(x => x.CanDeleteAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockRepository.Setup(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(categoryId);

            // Assert
            result.Should().BeTrue();

            _mockRepository.Verify(x => x.CanDeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_ShouldReturnFalse()
        {
            // Arrange
            var categoryId = 999;

            _mockRepository.Setup(x => x.CanDeleteAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(categoryId));

            _mockRepository.Verify(x => x.CanDeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_WhenCannotDelete_ShouldReturnFalse()
        {
            // Arrange
            var categoryId = 1;
            var category = new ServiceCategory
            {
                CategoryId = categoryId,
                CategoryName = "Test Category",
                IsActive = true
            };

            _mockRepository.Setup(x => x.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            _mockRepository.Setup(x => x.CanDeleteAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(categoryId));

            _mockRepository.Verify(x => x.CanDeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.DeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region GetActiveCategoriesAsync Tests

        [Fact]
        public async Task GetActiveCategoriesAsync_ShouldReturnActiveCategories()
        {
            // Arrange
            var categories = new List<ServiceCategory>
            {
                new ServiceCategory { CategoryId = 1, CategoryName = "Active Category 1", IsActive = true },
                new ServiceCategory { CategoryId = 2, CategoryName = "Active Category 2", IsActive = true }
            };

            // Mock cache to return null (not cached)
            object? cachedValue = null;
            _mockCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out cachedValue))
                .Returns(false);

            _mockRepository.Setup(x => x.GetActiveCategoriesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);

            // Mock cache Set method
            _mockCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>());

            // Act
            var result = await _service.GetActiveCategoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.All(c => c.IsActive == true).Should().BeTrue();

            _mockRepository.Verify(x => x.GetActiveCategoriesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region CanDeleteAsync Tests

        [Fact]
        public async Task CanDeleteAsync_WithValidId_ShouldReturnTrue()
        {
            // Arrange
            var categoryId = 1;

            _mockRepository.Setup(x => x.CanDeleteAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CanDeleteAsync(categoryId);

            // Assert
            result.Should().BeTrue();

            _mockRepository.Verify(x => x.CanDeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CanDeleteAsync_WhenCannotDelete_ShouldReturnFalse()
        {
            // Arrange
            var categoryId = 1;

            _mockRepository.Setup(x => x.CanDeleteAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CanDeleteAsync(categoryId);

            // Assert
            result.Should().BeFalse();

            _mockRepository.Verify(x => x.CanDeleteAsync(categoryId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion
    }
}
