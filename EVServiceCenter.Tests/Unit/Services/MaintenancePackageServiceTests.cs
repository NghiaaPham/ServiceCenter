using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Requests;
using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Services;
using EVServiceCenter.Core.Domains.Shared.Models;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Infrastructure.Domains.MaintenancePackages.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Services
{
    public class MaintenancePackageServiceTests
    {
        private readonly Mock<IMaintenancePackageQueryRepository> _mockQueryRepository;
        private readonly Mock<IMaintenancePackageCommandRepository> _mockCommandRepository;
        private readonly Mock<ILogger<MaintenancePackageService>> _mockLogger;
        private readonly MaintenancePackageService _service;

        public MaintenancePackageServiceTests()
        {
            _mockQueryRepository = new Mock<IMaintenancePackageQueryRepository>();
            _mockCommandRepository = new Mock<IMaintenancePackageCommandRepository>();
            _mockLogger = new Mock<ILogger<MaintenancePackageService>>();
            _service = new MaintenancePackageService(
                _mockQueryRepository.Object,
                _mockCommandRepository.Object,
                _mockLogger.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullQueryRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MaintenancePackageService(
                null!,
                _mockCommandRepository.Object,
                _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullCommandRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MaintenancePackageService(
                _mockQueryRepository.Object,
                null!,
                _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new MaintenancePackageService(
                _mockQueryRepository.Object,
                _mockCommandRepository.Object,
                null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var service = new MaintenancePackageService(
                _mockQueryRepository.Object,
                _mockCommandRepository.Object,
                _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        #endregion

        #region GetAllPackagesAsync Tests

        [Fact]
        public async Task GetAllPackagesAsync_WithValidQuery_ShouldReturnPagedResult()
        {
            // Arrange
            var query = new MaintenancePackageQueryDto
            {
                Page = 1,
                PageSize = 10,
                SearchTerm = "test",
                Status = PackageStatusEnum.Active
            };

            var expectedResult = new PagedResult<MaintenancePackageSummaryDto>
            {
                Items = new List<MaintenancePackageSummaryDto>
                {
                    new MaintenancePackageSummaryDto
                    {
                        PackageId = 1,
                        PackageCode = "PKG-001",
                        PackageName = "Test Package",
                        TotalPriceAfterDiscount = 1000000,
                        Status = PackageStatusEnum.Active
                    }
                },
                TotalCount = 1,
                Page = 1,
                PageSize = 10,
                TotalPages = 1
            };

            _mockQueryRepository.Setup(x => x.GetAllPackagesAsync(query, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _service.GetAllPackagesAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.TotalCount, result.TotalCount);
            Assert.Single(result.Items);
            Assert.Equal("PKG-001", result.Items.First().PackageCode);

            _mockQueryRepository.Verify(x => x.GetAllPackagesAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetAllPackagesAsync_WhenRepositoryThrowsException_ShouldRethrow()
        {
            // Arrange
            var query = new MaintenancePackageQueryDto();
            var expectedException = new Exception("Database error");

            _mockQueryRepository.Setup(x => x.GetAllPackagesAsync(query, It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.GetAllPackagesAsync(query));
            Assert.Equal("Database error", exception.Message);

            _mockQueryRepository.Verify(x => x.GetAllPackagesAsync(query, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetPackageByIdAsync Tests

        [Fact]
        public async Task GetPackageByIdAsync_WithExistingId_ShouldReturnPackage()
        {
            // Arrange
            var packageId = 1;
            var expectedPackage = new MaintenancePackageResponseDto
            {
                PackageId = packageId,
                PackageCode = "PKG-001",
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                Status = PackageStatusEnum.Active
            };

            _mockQueryRepository.Setup(x => x.GetPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPackage);

            // Act
            var result = await _service.GetPackageByIdAsync(packageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packageId, result.PackageId);
            Assert.Equal("PKG-001", result.PackageCode);

            _mockQueryRepository.Verify(x => x.GetPackageByIdAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPackageByIdAsync_WithNonExistentId_ShouldReturnNull()
        {
            // Arrange
            var packageId = 999;

            _mockQueryRepository.Setup(x => x.GetPackageByIdAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MaintenancePackageResponseDto?)null);

            // Act
            var result = await _service.GetPackageByIdAsync(packageId);

            // Assert
            Assert.Null(result);

            _mockQueryRepository.Verify(x => x.GetPackageByIdAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetPackageByCodeAsync Tests

        [Fact]
        public async Task GetPackageByCodeAsync_WithExistingCode_ShouldReturnPackage()
        {
            // Arrange
            var packageCode = "PKG-001";
            var expectedPackage = new MaintenancePackageResponseDto
            {
                PackageId = 1,
                PackageCode = packageCode,
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                Status = PackageStatusEnum.Active
            };

            _mockQueryRepository.Setup(x => x.GetPackageByCodeAsync(packageCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPackage);

            // Act
            var result = await _service.GetPackageByCodeAsync(packageCode);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(packageCode, result.PackageCode);

            _mockQueryRepository.Verify(x => x.GetPackageByCodeAsync(packageCode, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPackageByCodeAsync_WithNonExistentCode_ShouldReturnNull()
        {
            // Arrange
            var packageCode = "NON-EXISTENT";

            _mockQueryRepository.Setup(x => x.GetPackageByCodeAsync(packageCode, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MaintenancePackageResponseDto?)null);

            // Act
            var result = await _service.GetPackageByCodeAsync(packageCode);

            // Assert
            Assert.Null(result);

            _mockQueryRepository.Verify(x => x.GetPackageByCodeAsync(packageCode, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetPopularPackagesAsync Tests

        [Fact]
        public async Task GetPopularPackagesAsync_WithDefaultCount_ShouldReturnPopularPackages()
        {
            // Arrange
            var expectedPackages = new List<MaintenancePackageSummaryDto>
            {
                new MaintenancePackageSummaryDto
                {
                    PackageId = 1,
                    PackageCode = "PKG-POPULAR-1",
                    PackageName = "Popular Package 1",
                    IsPopularPackage = true,
                    Status = PackageStatusEnum.Active
                },
                new MaintenancePackageSummaryDto
                {
                    PackageId = 2,
                    PackageCode = "PKG-POPULAR-2",
                    PackageName = "Popular Package 2",
                    IsPopularPackage = true,
                    Status = PackageStatusEnum.Active
                }
            };

            _mockQueryRepository.Setup(x => x.GetPopularPackagesAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPackages);

            // Act
            var result = await _service.GetPopularPackagesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.True(p.IsPopularPackage));

            _mockQueryRepository.Verify(x => x.GetPopularPackagesAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetPopularPackagesAsync_WithCustomCount_ShouldReturnCorrectNumberOfPackages()
        {
            // Arrange
            var topCount = 3;
            var expectedPackages = new List<MaintenancePackageSummaryDto>
            {
                new MaintenancePackageSummaryDto { PackageId = 1, IsPopularPackage = true },
                new MaintenancePackageSummaryDto { PackageId = 2, IsPopularPackage = true },
                new MaintenancePackageSummaryDto { PackageId = 3, IsPopularPackage = true }
            };

            _mockQueryRepository.Setup(x => x.GetPopularPackagesAsync(topCount, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedPackages);

            // Act
            var result = await _service.GetPopularPackagesAsync(topCount);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(topCount, result.Count);

            _mockQueryRepository.Verify(x => x.GetPopularPackagesAsync(topCount, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region CreatePackageAsync Tests

        [Fact]
        public async Task CreatePackageAsync_WithValidRequest_ShouldCreatePackage()
        {
            // Arrange
            var request = new CreateMaintenancePackageRequestDto
            {
                PackageCode = "PKG-NEW-001",
                PackageName = "New Package",
                Description = "Test description",
                ValidityPeriodInDays = 365,
                ValidityMileage = 10000,
                TotalPriceAfterDiscount = 2000000,
                DiscountPercent = 20,
                IsPopularPackage = false,
                Status = PackageStatusEnum.Active,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            var createdByUserId = 1;
            var expectedResponse = new MaintenancePackageResponseDto
            {
                PackageId = 1,
                PackageCode = request.PackageCode,
                PackageName = request.PackageName,
                TotalPriceAfterDiscount = request.TotalPriceAfterDiscount,
                Status = request.Status
            };

            // Mock validation to pass
            _mockQueryRepository.Setup(x => x.IsPackageCodeExistsAsync(request.PackageCode, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepository.Setup(x => x.CreatePackageAsync(request, createdByUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.CreatePackageAsync(request, createdByUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.PackageCode, result.PackageCode);
            Assert.Equal(request.PackageName, result.PackageName);

            _mockQueryRepository.Verify(x => x.IsPackageCodeExistsAsync(request.PackageCode, null, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.CreatePackageAsync(request, createdByUserId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePackageAsync_WithDuplicatePackageCode_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new CreateMaintenancePackageRequestDto
            {
                PackageCode = "PKG-DUPLICATE",
                PackageName = "Duplicate Package",
                TotalPriceAfterDiscount = 1000000,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            var createdByUserId = 1;

            // Mock validation to fail - package code exists
            _mockQueryRepository.Setup(x => x.IsPackageCodeExistsAsync(request.PackageCode, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreatePackageAsync(request, createdByUserId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("đã tồn tại trong hệ thống", exception.Message);

            _mockQueryRepository.Verify(x => x.IsPackageCodeExistsAsync(request.PackageCode, null, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.CreatePackageAsync(It.IsAny<CreateMaintenancePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreatePackageAsync_WithEmptyPackageCode_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new CreateMaintenancePackageRequestDto
            {
                PackageCode = "", // Empty code
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            var createdByUserId = 1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreatePackageAsync(request, createdByUserId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("Mã gói không được để trống", exception.Message);

            _mockCommandRepository.Verify(x => x.CreatePackageAsync(It.IsAny<CreateMaintenancePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreatePackageAsync_WithEmptyServices_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new CreateMaintenancePackageRequestDto
            {
                PackageCode = "PKG-EMPTY-SERVICES",
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                IncludedServices = new List<PackageServiceItemRequestDto>() // Empty services
            };

            var createdByUserId = 1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreatePackageAsync(request, createdByUserId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("Gói phải chứa ít nhất 1 dịch vụ", exception.Message);

            _mockCommandRepository.Verify(x => x.CreatePackageAsync(It.IsAny<CreateMaintenancePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreatePackageAsync_WithNegativePrice_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new CreateMaintenancePackageRequestDto
            {
                PackageCode = "PKG-NEGATIVE-PRICE",
                PackageName = "Test Package",
                TotalPriceAfterDiscount = -1000, // Negative price
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            var createdByUserId = 1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreatePackageAsync(request, createdByUserId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("Giá gói không thể âm", exception.Message);

            _mockCommandRepository.Verify(x => x.CreatePackageAsync(It.IsAny<CreateMaintenancePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreatePackageAsync_WithInvalidDiscountPercent_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new CreateMaintenancePackageRequestDto
            {
                PackageCode = "PKG-INVALID-DISCOUNT",
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                DiscountPercent = 150, // Invalid discount > 100%
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            var createdByUserId = 1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreatePackageAsync(request, createdByUserId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("Phần trăm giảm giá phải từ 0-100%", exception.Message);

            _mockCommandRepository.Verify(x => x.CreatePackageAsync(It.IsAny<CreateMaintenancePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreatePackageAsync_WithNoValidityPeriodOrMileage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new CreateMaintenancePackageRequestDto
            {
                PackageCode = "PKG-NO-VALIDITY",
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                ValidityPeriodInDays = null, // No validity period
                ValidityMileage = null, // No validity mileage
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            var createdByUserId = 1;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CreatePackageAsync(request, createdByUserId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("Gói phải có ít nhất 1 điều kiện hết hạn", exception.Message);

            _mockCommandRepository.Verify(x => x.CreatePackageAsync(It.IsAny<CreateMaintenancePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region UpdatePackageAsync Tests

        [Fact]
        public async Task UpdatePackageAsync_WithValidRequest_ShouldUpdatePackage()
        {
            // Arrange
            var request = new UpdateMaintenancePackageRequestDto
            {
                PackageId = 1,
                PackageCode = "PKG-UPDATED-001",
                PackageName = "Updated Package",
                Description = "Updated description",
                ValidityPeriodInDays = 365,
                ValidityMileage = 10000,
                TotalPriceAfterDiscount = 2500000,
                DiscountPercent = 25,
                IsPopularPackage = true,
                Status = PackageStatusEnum.Active,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 2,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            var updatedByUserId = 1;
            var expectedResponse = new MaintenancePackageResponseDto
            {
                PackageId = request.PackageId,
                PackageCode = request.PackageCode,
                PackageName = request.PackageName,
                TotalPriceAfterDiscount = request.TotalPriceAfterDiscount,
                Status = request.Status
            };

            // Mock validation to pass
            _mockQueryRepository.Setup(x => x.PackageExistsAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.IsPackageCodeExistsAsync(request.PackageCode, request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepository.Setup(x => x.UpdatePackageAsync(request, updatedByUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.UpdatePackageAsync(request, updatedByUserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(request.PackageId, result.PackageId);
            Assert.Equal(request.PackageCode, result.PackageCode);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.IsPackageCodeExistsAsync(request.PackageCode, request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.UpdatePackageAsync(request, updatedByUserId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePackageAsync_WithNonExistentPackage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new UpdateMaintenancePackageRequestDto
            {
                PackageId = 999, // Non-existent package
                PackageCode = "PKG-NON-EXISTENT",
                PackageName = "Non-existent Package",
                TotalPriceAfterDiscount = 1000000,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            var updatedByUserId = 1;

            // Mock validation to fail - package doesn't exist
            _mockQueryRepository.Setup(x => x.PackageExistsAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.UpdatePackageAsync(request, updatedByUserId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("Không tìm thấy gói với ID", exception.Message);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.UpdatePackageAsync(It.IsAny<UpdateMaintenancePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePackageAsync_WithDuplicatePackageCode_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new UpdateMaintenancePackageRequestDto
            {
                PackageId = 1,
                PackageCode = "PKG-DUPLICATE",
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            var updatedByUserId = 1;

            // Mock validation to fail - package exists but code is duplicate
            _mockQueryRepository.Setup(x => x.PackageExistsAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.IsPackageCodeExistsAsync(request.PackageCode, request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.UpdatePackageAsync(request, updatedByUserId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("đã được sử dụng bởi gói khác", exception.Message);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.IsPackageCodeExistsAsync(request.PackageCode, request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.UpdatePackageAsync(It.IsAny<UpdateMaintenancePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region DeletePackageAsync Tests

        [Fact]
        public async Task DeletePackageAsync_WithValidPackage_ShouldDeletePackage()
        {
            // Arrange
            var packageId = 1;

            // Mock validation to pass
            _mockQueryRepository.Setup(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.HasActiveSubscriptionsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepository.Setup(x => x.SoftDeletePackageAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeletePackageAsync(packageId);

            // Assert
            Assert.True(result);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.SoftDeletePackageAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeletePackageAsync_WithNonExistentPackage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var packageId = 999;

            // Mock validation to fail - package doesn't exist
            _mockQueryRepository.Setup(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.DeletePackageAsync(packageId));

            Assert.Contains("Không thể xóa gói này vì đang có khách hàng sử dụng", exception.Message);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockCommandRepository.Verify(x => x.SoftDeletePackageAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeletePackageAsync_WithActiveSubscriptions_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var packageId = 1;

            // Mock validation to fail - package exists but has active subscriptions
            _mockQueryRepository.Setup(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.HasActiveSubscriptionsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.DeletePackageAsync(packageId));

            Assert.Contains("Không thể xóa gói này vì đang có khách hàng sử dụng", exception.Message);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.SoftDeletePackageAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeletePackageAsync_WhenCommandRepositoryReturnsFalse_ShouldReturnFalse()
        {
            // Arrange
            var packageId = 1;

            // Mock validation to pass
            _mockQueryRepository.Setup(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.HasActiveSubscriptionsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Mock command repository to return false (package not found for deletion)
            _mockCommandRepository.Setup(x => x.SoftDeletePackageAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.DeletePackageAsync(packageId);

            // Assert
            Assert.False(result);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.SoftDeletePackageAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region CanDeletePackageAsync Tests

        [Fact]
        public async Task CanDeletePackageAsync_WithValidPackage_ShouldReturnTrue()
        {
            // Arrange
            var packageId = 1;

            _mockQueryRepository.Setup(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.HasActiveSubscriptionsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CanDeletePackageAsync(packageId);

            // Assert
            Assert.True(result);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CanDeletePackageAsync_WithNonExistentPackage_ShouldReturnFalse()
        {
            // Arrange
            var packageId = 999;

            _mockQueryRepository.Setup(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CanDeletePackageAsync(packageId);

            // Assert
            Assert.False(result);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CanDeletePackageAsync_WithActiveSubscriptions_ShouldReturnFalse()
        {
            // Arrange
            var packageId = 1;

            _mockQueryRepository.Setup(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.HasActiveSubscriptionsAsync(packageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CanDeletePackageAsync(packageId);

            // Assert
            Assert.False(result);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionsAsync(packageId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region ValidateCreatePackageRequestAsync Tests

        [Fact]
        public async Task ValidateCreatePackageRequestAsync_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var request = new CreateMaintenancePackageRequestDto
            {
                PackageCode = "PKG-VALID-001",
                PackageName = "Valid Package",
                TotalPriceAfterDiscount = 1000000,
                ValidityPeriodInDays = 365,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            _mockQueryRepository.Setup(x => x.IsPackageCodeExistsAsync(request.PackageCode, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ValidateCreatePackageRequestAsync(request);

            // Assert
            Assert.True(result.IsValid);
            Assert.Null(result.ErrorMessage);

            _mockQueryRepository.Verify(x => x.IsPackageCodeExistsAsync(request.PackageCode, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ValidateCreatePackageRequestAsync_WithEmptyPackageCode_ShouldReturnInvalid()
        {
            // Arrange
            var request = new CreateMaintenancePackageRequestDto
            {
                PackageCode = "", // Empty code
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            // Act
            var result = await _service.ValidateCreatePackageRequestAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Mã gói không được để trống", result.ErrorMessage);

            _mockQueryRepository.Verify(x => x.IsPackageCodeExistsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ValidateCreatePackageRequestAsync_WithDuplicateServices_ShouldReturnInvalid()
        {
            // Arrange
            var request = new CreateMaintenancePackageRequestDto
            {
                PackageCode = "PKG-DUPLICATE-SERVICES",
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                ValidityPeriodInDays = 365,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    },
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1, // Duplicate service ID
                        QuantityInPackage = 2,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            // Act
            var result = await _service.ValidateCreatePackageRequestAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Có dịch vụ trùng lặp trong gói", result.ErrorMessage);
            Assert.Contains("ServiceId 1", result.ErrorMessage);
        }

        [Fact]
        public async Task ValidateCreatePackageRequestAsync_WithZeroQuantity_ShouldReturnInvalid()
        {
            // Arrange
            var request = new CreateMaintenancePackageRequestDto
            {
                PackageCode = "PKG-ZERO-QUANTITY",
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                ValidityPeriodInDays = 365,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 0, // Zero quantity
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            // Act
            var result = await _service.ValidateCreatePackageRequestAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Số lượng dịch vụ trong gói phải > 0", result.ErrorMessage);
        }

        #endregion

        #region ValidateUpdatePackageRequestAsync Tests

        [Fact]
        public async Task ValidateUpdatePackageRequestAsync_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var request = new UpdateMaintenancePackageRequestDto
            {
                PackageId = 1,
                PackageCode = "PKG-UPDATED-001",
                PackageName = "Updated Package",
                TotalPriceAfterDiscount = 1000000,
                ValidityPeriodInDays = 365,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            _mockQueryRepository.Setup(x => x.PackageExistsAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.IsPackageCodeExistsAsync(request.PackageCode, request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ValidateUpdatePackageRequestAsync(request);

            // Assert
            Assert.True(result.IsValid);
            Assert.Null(result.ErrorMessage);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.IsPackageCodeExistsAsync(request.PackageCode, request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ValidateUpdatePackageRequestAsync_WithNonExistentPackage_ShouldReturnInvalid()
        {
            // Arrange
            var request = new UpdateMaintenancePackageRequestDto
            {
                PackageId = 999, // Non-existent package
                PackageCode = "PKG-NON-EXISTENT",
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                IncludedServices = new List<PackageServiceItemRequestDto>
                {
                    new PackageServiceItemRequestDto
                    {
                        ServiceId = 1,
                        QuantityInPackage = 1,
                        IsIncludedInPackagePrice = true
                    }
                }
            };

            _mockQueryRepository.Setup(x => x.PackageExistsAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ValidateUpdatePackageRequestAsync(request);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Không tìm thấy gói với ID", result.ErrorMessage);

            _mockQueryRepository.Verify(x => x.PackageExistsAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.IsPackageCodeExistsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}
