using EVServiceCenter.Core.Domains.MaintenancePackages.DTOs.Responses;
using EVServiceCenter.Core.Domains.MaintenancePackages.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Requests;
using EVServiceCenter.Core.Domains.PackageSubscriptions.DTOs.Responses;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.PackageSubscriptions.Interfaces.Services;
using EVServiceCenter.Core.Enums;
using EVServiceCenter.Infrastructure.Domains.PackageSubscriptions.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Services
{
    public class PackageSubscriptionServiceTests
    {
        private readonly Mock<IPackageSubscriptionQueryRepository> _mockQueryRepository;
        private readonly Mock<IPackageSubscriptionCommandRepository> _mockCommandRepository;
        private readonly Mock<IMaintenancePackageQueryRepository> _mockPackageQueryRepository;
        private readonly Mock<ILogger<PackageSubscriptionService>> _mockLogger;
        private readonly PackageSubscriptionService _service;

        public PackageSubscriptionServiceTests()
        {
            _mockQueryRepository = new Mock<IPackageSubscriptionQueryRepository>();
            _mockCommandRepository = new Mock<IPackageSubscriptionCommandRepository>();
            _mockPackageQueryRepository = new Mock<IMaintenancePackageQueryRepository>();
            _mockLogger = new Mock<ILogger<PackageSubscriptionService>>();
            _service = new PackageSubscriptionService(
                _mockQueryRepository.Object,
                _mockCommandRepository.Object,
                _mockPackageQueryRepository.Object,
                _mockLogger.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullQueryRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PackageSubscriptionService(
                null!,
                _mockCommandRepository.Object,
                _mockPackageQueryRepository.Object,
                _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullCommandRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PackageSubscriptionService(
                _mockQueryRepository.Object,
                null!,
                _mockPackageQueryRepository.Object,
                _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullPackageQueryRepository_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PackageSubscriptionService(
                _mockQueryRepository.Object,
                _mockCommandRepository.Object,
                null!,
                _mockLogger.Object));
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new PackageSubscriptionService(
                _mockQueryRepository.Object,
                _mockCommandRepository.Object,
                _mockPackageQueryRepository.Object,
                null!));
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var service = new PackageSubscriptionService(
                _mockQueryRepository.Object,
                _mockCommandRepository.Object,
                _mockPackageQueryRepository.Object,
                _mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        #endregion

        #region GetMySubscriptionsAsync Tests

        [Fact]
        public async Task GetMySubscriptionsAsync_WithValidCustomerId_ShouldReturnSubscriptions()
        {
            // Arrange
            var customerId = 1;
            var expectedSubscriptions = new List<PackageSubscriptionSummaryDto>
            {
                new PackageSubscriptionSummaryDto
                {
                    SubscriptionId = 1,
                    PackageCode = "PKG-001",
                    PackageName = "Test Package",
                    VehiclePlateNumber = "ABC-123",
                    Status = SubscriptionStatusEnum.Active
                }
            };

            _mockQueryRepository.Setup(x => x.GetCustomerSubscriptionsAsync(customerId, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSubscriptions);

            // Act
            var result = await _service.GetMySubscriptionsAsync(customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("PKG-001", result.First().PackageCode);

            _mockQueryRepository.Verify(x => x.GetCustomerSubscriptionsAsync(customerId, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMySubscriptionsAsync_WithStatusFilter_ShouldReturnFilteredSubscriptions()
        {
            // Arrange
            var customerId = 1;
            var statusFilter = SubscriptionStatusEnum.Active;
            var expectedSubscriptions = new List<PackageSubscriptionSummaryDto>
            {
                new PackageSubscriptionSummaryDto
                {
                    SubscriptionId = 1,
                    Status = SubscriptionStatusEnum.Active
                }
            };

            _mockQueryRepository.Setup(x => x.GetCustomerSubscriptionsAsync(customerId, statusFilter, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSubscriptions);

            // Act
            var result = await _service.GetMySubscriptionsAsync(customerId, statusFilter);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(SubscriptionStatusEnum.Active, result.First().Status);

            _mockQueryRepository.Verify(x => x.GetCustomerSubscriptionsAsync(customerId, statusFilter, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetMySubscriptionsAsync_WhenRepositoryThrowsException_ShouldRethrow()
        {
            // Arrange
            var customerId = 1;
            var expectedException = new Exception("Database error");

            _mockQueryRepository.Setup(x => x.GetCustomerSubscriptionsAsync(customerId, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _service.GetMySubscriptionsAsync(customerId));
            Assert.Equal("Database error", exception.Message);

            _mockQueryRepository.Verify(x => x.GetCustomerSubscriptionsAsync(customerId, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetSubscriptionDetailsAsync Tests

        [Fact]
        public async Task GetSubscriptionDetailsAsync_WithValidOwnership_ShouldReturnSubscription()
        {
            // Arrange
            var subscriptionId = 1;
            var customerId = 1;
            var expectedSubscription = new PackageSubscriptionResponseDto
            {
                SubscriptionId = subscriptionId,
                CustomerId = customerId,
                PackageCode = "PKG-001",
                Status = SubscriptionStatusEnum.Active
            };

            _mockQueryRepository.Setup(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSubscription);

            // Act
            var result = await _service.GetSubscriptionDetailsAsync(subscriptionId, customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(subscriptionId, result.SubscriptionId);
            Assert.Equal(customerId, result.CustomerId);

            _mockQueryRepository.Verify(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionDetailsAsync_WithInvalidOwnership_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var subscriptionId = 1;
            var customerId = 1;

            _mockQueryRepository.Setup(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.GetSubscriptionDetailsAsync(subscriptionId, customerId));

            Assert.Contains("Bạn không có quyền xem subscription này", exception.Message);

            _mockQueryRepository.Verify(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region GetActiveSubscriptionsForVehicleAsync Tests

        [Fact]
        public async Task GetActiveSubscriptionsForVehicleAsync_WithValidVehicle_ShouldReturnSubscriptions()
        {
            // Arrange
            var vehicleId = 1;
            var customerId = 1;
            var expectedSubscriptions = new List<PackageSubscriptionSummaryDto>
            {
                new PackageSubscriptionSummaryDto
                {
                    SubscriptionId = 1,
                    VehiclePlateNumber = "ABC-123",
                    Status = SubscriptionStatusEnum.Active
                }
            };

            _mockQueryRepository.Setup(x => x.GetActiveSubscriptionsForVehicleAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedSubscriptions);

            // Act
            var result = await _service.GetActiveSubscriptionsForVehicleAsync(vehicleId, customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("ABC-123", result.First().VehiclePlateNumber);

            _mockQueryRepository.Verify(x => x.GetActiveSubscriptionsForVehicleAsync(vehicleId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region GetSubscriptionUsageDetailsAsync Tests

        [Fact]
        public async Task GetSubscriptionUsageDetailsAsync_WithValidOwnership_ShouldReturnUsageDetails()
        {
            // Arrange
            var subscriptionId = 1;
            var customerId = 1;
            var expectedUsage = new List<PackageServiceUsageDto>
            {
                new PackageServiceUsageDto
                {
                    UsageId = 1,
                    ServiceId = 1,
                    ServiceName = "Test Service",
                    TotalAllowedQuantity = 3,
                    UsedQuantity = 1,
                    RemainingQuantity = 2
                }
            };

            _mockQueryRepository.Setup(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.GetSubscriptionUsageDetailsAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUsage);

            // Act
            var result = await _service.GetSubscriptionUsageDetailsAsync(subscriptionId, customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Service", result.First().ServiceName);
            Assert.Equal(2, result.First().RemainingQuantity);

            _mockQueryRepository.Verify(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.GetSubscriptionUsageDetailsAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionUsageDetailsAsync_WithInvalidOwnership_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var subscriptionId = 1;
            var customerId = 1;

            _mockQueryRepository.Setup(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.GetSubscriptionUsageDetailsAsync(subscriptionId, customerId));

            Assert.Contains("Bạn không có quyền xem usage của subscription này", exception.Message);

            _mockQueryRepository.Verify(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.GetSubscriptionUsageDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region PurchasePackageAsync Tests

        [Fact]
        public async Task PurchasePackageAsync_WithValidRequest_ShouldCreateSubscription()
        {
            // Arrange
            var request = new PurchasePackageRequestDto
            {
                PackageId = 1,
                VehicleId = 1,
                PaymentMethod = "Cash",
                AmountPaid = 1000000,
                CustomerNotes = "Test purchase"
            };
            var customerId = 1;

            var package = new MaintenancePackageResponseDto
            {
                PackageId = request.PackageId,
                PackageCode = "PKG-001",
                PackageName = "Test Package",
                TotalPriceAfterDiscount = 1000000,
                Status = PackageStatusEnum.Active
            };

            var expectedResponse = new PackageSubscriptionResponseDto
            {
                SubscriptionId = 1,
                CustomerId = customerId,
                PackageId = request.PackageId,
                PackageCode = package.PackageCode,
                Status = SubscriptionStatusEnum.Active
            };

            // Mock validation to pass
            _mockPackageQueryRepository.Setup(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(package);
            _mockQueryRepository.Setup(x => x.HasActiveSubscriptionForPackageAsync(customerId, request.VehicleId, request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockCommandRepository.Setup(x => x.PurchasePackageAsync(request, customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _service.PurchasePackageAsync(request, customerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.CustomerId);
            Assert.Equal(request.PackageId, result.PackageId);

            _mockPackageQueryRepository.Verify(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionForPackageAsync(customerId, request.VehicleId, request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.PurchasePackageAsync(request, customerId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PurchasePackageAsync_WithNonExistentPackage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new PurchasePackageRequestDto
            {
                PackageId = 999, // Non-existent package
                VehicleId = 1,
                PaymentMethod = "Cash",
                AmountPaid = 1000000
            };
            var customerId = 1;

            // Mock validation to fail - package doesn't exist
            _mockPackageQueryRepository.Setup(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MaintenancePackageResponseDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.PurchasePackageAsync(request, customerId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("Không tìm thấy gói dịch vụ với ID", exception.Message);

            _mockPackageQueryRepository.Verify(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.PurchasePackageAsync(It.IsAny<PurchasePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PurchasePackageAsync_WithInactivePackage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new PurchasePackageRequestDto
            {
                PackageId = 1,
                VehicleId = 1,
                PaymentMethod = "Cash",
                AmountPaid = 1000000
            };
            var customerId = 1;

            var package = new MaintenancePackageResponseDto
            {
                PackageId = request.PackageId,
                Status = PackageStatusEnum.Inactive // Inactive package
            };

            // Mock validation to fail - package is inactive
            _mockPackageQueryRepository.Setup(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(package);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.PurchasePackageAsync(request, customerId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("Gói dịch vụ này hiện không còn hoạt động", exception.Message);

            _mockPackageQueryRepository.Verify(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.PurchasePackageAsync(It.IsAny<PurchasePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PurchasePackageAsync_WithExistingActiveSubscription_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new PurchasePackageRequestDto
            {
                PackageId = 1,
                VehicleId = 1,
                PaymentMethod = "Cash",
                AmountPaid = 1000000
            };
            var customerId = 1;

            var package = new MaintenancePackageResponseDto
            {
                PackageId = request.PackageId,
                Status = PackageStatusEnum.Active
            };

            // Mock validation to fail - has active subscription
            _mockPackageQueryRepository.Setup(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(package);
            _mockQueryRepository.Setup(x => x.HasActiveSubscriptionForPackageAsync(customerId, request.VehicleId, request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.PurchasePackageAsync(request, customerId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("Bạn đã có subscription active cho gói này trên xe này", exception.Message);

            _mockPackageQueryRepository.Verify(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionForPackageAsync(customerId, request.VehicleId, request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.PurchasePackageAsync(It.IsAny<PurchasePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PurchasePackageAsync_WithInsufficientPayment_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new PurchasePackageRequestDto
            {
                PackageId = 1,
                VehicleId = 1,
                PaymentMethod = "Cash",
                AmountPaid = 500000 // Insufficient payment
            };
            var customerId = 1;

            var package = new MaintenancePackageResponseDto
            {
                PackageId = request.PackageId,
                TotalPriceAfterDiscount = 1000000, // Package costs 1M
                Status = PackageStatusEnum.Active
            };

            // Mock validation to fail - insufficient payment
            _mockPackageQueryRepository.Setup(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(package);
            _mockQueryRepository.Setup(x => x.HasActiveSubscriptionForPackageAsync(customerId, request.VehicleId, request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.PurchasePackageAsync(request, customerId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("Số tiền thanh toán", exception.Message);
            Assert.Contains("không đủ", exception.Message);

            _mockPackageQueryRepository.Verify(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionForPackageAsync(customerId, request.VehicleId, request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.PurchasePackageAsync(It.IsAny<PurchasePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task PurchasePackageAsync_WithEmptyPaymentMethod_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new PurchasePackageRequestDto
            {
                PackageId = 1,
                VehicleId = 1,
                PaymentMethod = "", // Empty payment method
                AmountPaid = 1000000
            };
            var customerId = 1;

            var package = new MaintenancePackageResponseDto
            {
                PackageId = request.PackageId,
                TotalPriceAfterDiscount = 1000000,
                Status = PackageStatusEnum.Active
            };

            // Mock validation to fail - empty payment method
            _mockPackageQueryRepository.Setup(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(package);
            _mockQueryRepository.Setup(x => x.HasActiveSubscriptionForPackageAsync(customerId, request.VehicleId, request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.PurchasePackageAsync(request, customerId));

            Assert.Contains("Validation failed", exception.Message);
            Assert.Contains("Phương thức thanh toán không được để trống", exception.Message);

            _mockPackageQueryRepository.Verify(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionForPackageAsync(customerId, request.VehicleId, request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.PurchasePackageAsync(It.IsAny<PurchasePackageRequestDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region CancelSubscriptionAsync Tests

        [Fact]
        public async Task CancelSubscriptionAsync_WithValidSubscription_ShouldCancelSubscription()
        {
            // Arrange
            var subscriptionId = 1;
            var customerId = 1;
            var cancellationReason = "Customer request";

            var subscription = new PackageSubscriptionResponseDto
            {
                SubscriptionId = subscriptionId,
                CustomerId = customerId,
                Status = SubscriptionStatusEnum.Active
            };

            // Mock validation to pass
            _mockQueryRepository.Setup(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            _mockCommandRepository.Setup(x => x.CancelSubscriptionAsync(subscriptionId, cancellationReason, customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CancelSubscriptionAsync(subscriptionId, cancellationReason, customerId);

            // Assert
            Assert.True(result);

            _mockQueryRepository.Verify(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.CancelSubscriptionAsync(subscriptionId, cancellationReason, customerId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CancelSubscriptionAsync_WithInvalidOwnership_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var subscriptionId = 1;
            var customerId = 1;
            var cancellationReason = "Customer request";

            _mockQueryRepository.Setup(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
                _service.CancelSubscriptionAsync(subscriptionId, cancellationReason, customerId));

            Assert.Contains("Bạn không có quyền hủy subscription này", exception.Message);

            _mockQueryRepository.Verify(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockCommandRepository.Verify(x => x.CancelSubscriptionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CancelSubscriptionAsync_WithNonExistentSubscription_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var subscriptionId = 999; // Non-existent subscription
            var customerId = 1;
            var cancellationReason = "Customer request";

            _mockQueryRepository.Setup(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PackageSubscriptionResponseDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CancelSubscriptionAsync(subscriptionId, cancellationReason, customerId));

            Assert.Contains("Không tìm thấy subscription", exception.Message);

            _mockQueryRepository.Verify(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.CancelSubscriptionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CancelSubscriptionAsync_WithCancelledSubscription_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var subscriptionId = 1;
            var customerId = 1;
            var cancellationReason = "Customer request";

            var subscription = new PackageSubscriptionResponseDto
            {
                SubscriptionId = subscriptionId,
                CustomerId = customerId,
                Status = SubscriptionStatusEnum.Cancelled // Already cancelled
            };

            _mockQueryRepository.Setup(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.CancelSubscriptionAsync(subscriptionId, cancellationReason, customerId));

            Assert.Contains("Chỉ có thể hủy subscription đang Active hoặc Suspended", exception.Message);

            _mockQueryRepository.Verify(x => x.IsSubscriptionOwnedByCustomerAsync(subscriptionId, customerId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.CancelSubscriptionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region SuspendSubscriptionAsync Tests

        [Fact]
        public async Task SuspendSubscriptionAsync_WithValidActiveSubscription_ShouldSuspendSubscription()
        {
            // Arrange
            var subscriptionId = 1;
            var reason = "Customer request to suspend";

            var subscription = new PackageSubscriptionResponseDto
            {
                SubscriptionId = subscriptionId,
                Status = SubscriptionStatusEnum.Active,
                StatusDisplayName = "Đang hoạt động"
            };

            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            _mockCommandRepository.Setup(x => x.SuspendSubscriptionAsync(subscriptionId, reason, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.SuspendSubscriptionAsync(subscriptionId, reason);

            // Assert
            Assert.True(result);

            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.SuspendSubscriptionAsync(subscriptionId, reason, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SuspendSubscriptionAsync_WithNonExistentSubscription_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var subscriptionId = 999; // Non-existent subscription
            var reason = "Customer request to suspend";

            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PackageSubscriptionResponseDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.SuspendSubscriptionAsync(subscriptionId, reason));

            Assert.Contains("Không tìm thấy subscription", exception.Message);

            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.SuspendSubscriptionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SuspendSubscriptionAsync_WithNonActiveSubscription_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var subscriptionId = 1;
            var reason = "Customer request to suspend";

            var subscription = new PackageSubscriptionResponseDto
            {
                SubscriptionId = subscriptionId,
                Status = SubscriptionStatusEnum.Cancelled, // Not active
                StatusDisplayName = "Đã hủy"
            };

            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.SuspendSubscriptionAsync(subscriptionId, reason));

            Assert.Contains("Chỉ có thể tạm dừng subscription đang Active", exception.Message);

            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.SuspendSubscriptionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task SuspendSubscriptionAsync_WithShortReason_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var subscriptionId = 1;
            var reason = "Short"; // Too short reason

            var subscription = new PackageSubscriptionResponseDto
            {
                SubscriptionId = subscriptionId,
                Status = SubscriptionStatusEnum.Active,
                StatusDisplayName = "Đang hoạt động"
            };

            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.SuspendSubscriptionAsync(subscriptionId, reason));

            Assert.Contains("Lý do tạm dừng phải có ít nhất 10 ký tự", exception.Message);

            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.SuspendSubscriptionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ReactivateSubscriptionAsync Tests

        [Fact]
        public async Task ReactivateSubscriptionAsync_WithValidSuspendedSubscription_ShouldReactivateSubscription()
        {
            // Arrange
            var subscriptionId = 1;

            var subscription = new PackageSubscriptionResponseDto
            {
                SubscriptionId = subscriptionId,
                Status = SubscriptionStatusEnum.Suspended,
                StatusDisplayName = "Tạm ngưng",
                ExpiryDate = DateTime.UtcNow.AddDays(30) // Not expired
            };

            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            _mockCommandRepository.Setup(x => x.ReactivateSubscriptionAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.ReactivateSubscriptionAsync(subscriptionId);

            // Assert
            Assert.True(result);

            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.ReactivateSubscriptionAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ReactivateSubscriptionAsync_WithNonExistentSubscription_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var subscriptionId = 999; // Non-existent subscription

            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PackageSubscriptionResponseDto?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.ReactivateSubscriptionAsync(subscriptionId));

            Assert.Contains("Không tìm thấy subscription", exception.Message);

            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.ReactivateSubscriptionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReactivateSubscriptionAsync_WithNonSuspendedSubscription_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var subscriptionId = 1;

            var subscription = new PackageSubscriptionResponseDto
            {
                SubscriptionId = subscriptionId,
                Status = SubscriptionStatusEnum.Active, // Not suspended
                StatusDisplayName = "Đang hoạt động"
            };

            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.ReactivateSubscriptionAsync(subscriptionId));

            Assert.Contains("Chỉ có thể kích hoạt lại subscription đang Suspended", exception.Message);

            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.ReactivateSubscriptionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ReactivateSubscriptionAsync_WithExpiredSubscription_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var subscriptionId = 1;

            var subscription = new PackageSubscriptionResponseDto
            {
                SubscriptionId = subscriptionId,
                Status = SubscriptionStatusEnum.Suspended,
                StatusDisplayName = "Tạm ngưng",
                ExpiryDate = DateTime.UtcNow.AddDays(-1) // Expired
            };

            _mockQueryRepository.Setup(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(subscription);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.ReactivateSubscriptionAsync(subscriptionId));

            Assert.Contains("Không thể kích hoạt lại subscription đã hết hạn", exception.Message);

            _mockQueryRepository.Verify(x => x.GetSubscriptionByIdAsync(subscriptionId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.ReactivateSubscriptionAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region UpdateServiceUsageAfterAppointmentAsync Tests

        [Fact]
        public async Task UpdateServiceUsageAfterAppointmentAsync_WithValidUsage_ShouldUpdateUsage()
        {
            // Arrange
            var subscriptionId = 1;
            var serviceId = 1;
            var quantityUsed = 1;
            var appointmentId = 1;

            _mockQueryRepository.Setup(x => x.HasRemainingUsageForServiceAsync(subscriptionId, serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockCommandRepository.Setup(x => x.UpdateServiceUsageAsync(subscriptionId, serviceId, quantityUsed, appointmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.UpdateServiceUsageAfterAppointmentAsync(subscriptionId, serviceId, quantityUsed, appointmentId);

            // Assert
            Assert.True(result);

            _mockQueryRepository.Verify(x => x.HasRemainingUsageForServiceAsync(subscriptionId, serviceId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.UpdateServiceUsageAsync(subscriptionId, serviceId, quantityUsed, appointmentId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateServiceUsageAfterAppointmentAsync_WithNoRemainingUsage_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var subscriptionId = 1;
            var serviceId = 1;
            var quantityUsed = 1;
            var appointmentId = 1;

            _mockQueryRepository.Setup(x => x.HasRemainingUsageForServiceAsync(subscriptionId, serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false); // No remaining usage

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _service.UpdateServiceUsageAfterAppointmentAsync(subscriptionId, serviceId, quantityUsed, appointmentId));

            Assert.Contains("Subscription không còn lượt sử dụng cho dịch vụ này", exception.Message);

            _mockQueryRepository.Verify(x => x.HasRemainingUsageForServiceAsync(subscriptionId, serviceId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.UpdateServiceUsageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region ValidatePurchaseRequestAsync Tests

        [Fact]
        public async Task ValidatePurchaseRequestAsync_WithValidRequest_ShouldReturnValid()
        {
            // Arrange
            var request = new PurchasePackageRequestDto
            {
                PackageId = 1,
                VehicleId = 1,
                PaymentMethod = "Cash",
                AmountPaid = 1000000
            };
            var customerId = 1;

            var package = new MaintenancePackageResponseDto
            {
                PackageId = request.PackageId,
                TotalPriceAfterDiscount = 1000000,
                Status = PackageStatusEnum.Active
            };

            _mockPackageQueryRepository.Setup(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(package);
            _mockQueryRepository.Setup(x => x.HasActiveSubscriptionForPackageAsync(customerId, request.VehicleId, request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.ValidatePurchaseRequestAsync(request, customerId);

            // Assert
            Assert.True(result.IsValid);
            Assert.Null(result.ErrorMessage);

            _mockPackageQueryRepository.Verify(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionForPackageAsync(customerId, request.VehicleId, request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ValidatePurchaseRequestAsync_WithNonExistentPackage_ShouldReturnInvalid()
        {
            // Arrange
            var request = new PurchasePackageRequestDto
            {
                PackageId = 999, // Non-existent package
                VehicleId = 1,
                PaymentMethod = "Cash",
                AmountPaid = 1000000
            };
            var customerId = 1;

            _mockPackageQueryRepository.Setup(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((MaintenancePackageResponseDto?)null);

            // Act
            var result = await _service.ValidatePurchaseRequestAsync(request, customerId);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("Không tìm thấy gói dịch vụ với ID", result.ErrorMessage);

            _mockPackageQueryRepository.Verify(x => x.GetPackageByIdAsync(request.PackageId, It.IsAny<CancellationToken>()), Times.Once);
            _mockQueryRepository.Verify(x => x.HasActiveSubscriptionForPackageAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion

        #region CanUseServiceFromSubscriptionAsync Tests

        [Fact]
        public async Task CanUseServiceFromSubscriptionAsync_WithRemainingUsage_ShouldReturnTrue()
        {
            // Arrange
            var subscriptionId = 1;
            var serviceId = 1;

            _mockQueryRepository.Setup(x => x.HasRemainingUsageForServiceAsync(subscriptionId, serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _service.CanUseServiceFromSubscriptionAsync(subscriptionId, serviceId);

            // Assert
            Assert.True(result);

            _mockQueryRepository.Verify(x => x.HasRemainingUsageForServiceAsync(subscriptionId, serviceId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CanUseServiceFromSubscriptionAsync_WithNoRemainingUsage_ShouldReturnFalse()
        {
            // Arrange
            var subscriptionId = 1;
            var serviceId = 1;

            _mockQueryRepository.Setup(x => x.HasRemainingUsageForServiceAsync(subscriptionId, serviceId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.CanUseServiceFromSubscriptionAsync(subscriptionId, serviceId);

            // Assert
            Assert.False(result);

            _mockQueryRepository.Verify(x => x.HasRemainingUsageForServiceAsync(subscriptionId, serviceId, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion
    }
}
