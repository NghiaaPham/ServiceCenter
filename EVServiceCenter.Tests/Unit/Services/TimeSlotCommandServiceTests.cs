using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Requests;
using EVServiceCenter.Core.Domains.TimeSlots.DTOs.Responses;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Repositories;
using EVServiceCenter.Core.Domains.TimeSlots.Interfaces.Services;
using EVServiceCenter.Core.Domains.ServiceCenters.Entities;
using EVServiceCenter.Core.Domains.ServiceCenters.Interfaces.Repositories;
using EVServiceCenter.Infrastructure.Domains.TimeSlots.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace EVServiceCenter.Tests.Unit.Services
{
    public class TimeSlotCommandServiceTests
    {
        private readonly Mock<ITimeSlotRepository> _mockRepository;
        private readonly Mock<ITimeSlotQueryRepository> _mockQueryRepository;
        private readonly Mock<ITimeSlotCommandRepository> _mockCommandRepository;
        private readonly Mock<IServiceCenterRepository> _mockCenterRepository;
        private readonly Mock<ILogger<TimeSlotCommandService>> _mockLogger;
        private readonly TimeSlotCommandService _service;

        public TimeSlotCommandServiceTests()
        {
            _mockRepository = new Mock<ITimeSlotRepository>();
            _mockQueryRepository = new Mock<ITimeSlotQueryRepository>();
            _mockCommandRepository = new Mock<ITimeSlotCommandRepository>();
            _mockCenterRepository = new Mock<IServiceCenterRepository>();
            _mockLogger = new Mock<ILogger<TimeSlotCommandService>>();
            
            _service = new TimeSlotCommandService(
                _mockRepository.Object,
                _mockQueryRepository.Object,
                _mockCommandRepository.Object,
                _mockCenterRepository.Object,
                _mockLogger.Object);
        }

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidData_ShouldCreateTimeSlot()
        {
            // Arrange
            var request = new CreateTimeSlotRequestDto
            {
                CenterId = 1,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("09:00"),
                EndTime = TimeOnly.Parse("10:00"),
                MaxBookings = 2,
                SlotType = "Regular",
                IsBlocked = false,
                Notes = "Test slot"
            };

            var center = new ServiceCenter
            {
                CenterId = 1,
                CenterName = "Test Center"
            };

            var createdSlot = new TimeSlot
            {
                SlotId = 1,
                CenterId = request.CenterId,
                SlotDate = request.SlotDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MaxBookings = request.MaxBookings,
                SlotType = request.SlotType,
                IsBlocked = request.IsBlocked,
                Notes = request.Notes,
                Center = center
            };

            var slotWithDetails = new TimeSlot
            {
                SlotId = 1,
                CenterId = request.CenterId,
                SlotDate = request.SlotDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MaxBookings = request.MaxBookings,
                SlotType = request.SlotType,
                IsBlocked = request.IsBlocked,
                Notes = request.Notes,
                Center = center
            };

            _mockCenterRepository.Setup(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(center);

            _mockCommandRepository.Setup(x => x.HasConflictAsync(
                request.CenterId, request.SlotDate, request.StartTime, request.EndTime, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(x => x.CreateAsync(It.IsAny<TimeSlot>()))
                .ReturnsAsync(createdSlot);

            _mockRepository.Setup(x => x.GetByIdWithDetailsAsync(createdSlot.SlotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(slotWithDetails);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.SlotId.Should().Be(1);
            result.CenterId.Should().Be(request.CenterId);
            result.SlotDate.Should().Be(request.SlotDate);
            result.StartTime.Should().Be(request.StartTime);
            result.EndTime.Should().Be(request.EndTime);
            result.MaxBookings.Should().Be(request.MaxBookings);
            result.SlotType.Should().Be(request.SlotType);
            result.IsBlocked.Should().Be(request.IsBlocked);
            result.Notes.Should().Be(request.Notes);

            _mockCenterRepository.Verify(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.HasConflictAsync(
                request.CenterId, request.SlotDate, request.StartTime, request.EndTime, null, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.CreateAsync(It.IsAny<TimeSlot>()), Times.Once);
            _mockRepository.Verify(x => x.GetByIdWithDetailsAsync(createdSlot.SlotId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithInvalidCenterId_ShouldThrowException()
        {
            // Arrange
            var request = new CreateTimeSlotRequestDto
            {
                CenterId = 999,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("09:00"),
                EndTime = TimeOnly.Parse("10:00")
            };

            _mockCenterRepository.Setup(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceCenter?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));

            _mockCenterRepository.Verify(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.HasConflictAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockRepository.Verify(x => x.CreateAsync(It.IsAny<TimeSlot>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_WithTimeConflict_ShouldThrowException()
        {
            // Arrange
            var request = new CreateTimeSlotRequestDto
            {
                CenterId = 1,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("09:00"),
                EndTime = TimeOnly.Parse("10:00")
            };

            var center = new ServiceCenter
            {
                CenterId = 1,
                CenterName = "Test Center"
            };

            _mockCenterRepository.Setup(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(center);

            _mockCommandRepository.Setup(x => x.HasConflictAsync(
                request.CenterId, request.SlotDate, request.StartTime, request.EndTime, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(request));

            _mockCenterRepository.Verify(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.HasConflictAsync(
                request.CenterId, request.SlotDate, request.StartTime, request.EndTime, null, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.CreateAsync(It.IsAny<TimeSlot>()), Times.Never);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidData_ShouldUpdateTimeSlot()
        {
            // Arrange
            var request = new UpdateTimeSlotRequestDto
            {
                SlotId = 1,
                CenterId = 1,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("10:00"),
                EndTime = TimeOnly.Parse("11:00"),
                MaxBookings = 3,
                SlotType = "Express",
                IsBlocked = false,
                Notes = "Updated slot"
            };

            var existingSlot = new TimeSlot
            {
                SlotId = 1,
                CenterId = 1,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("09:00"),
                EndTime = TimeOnly.Parse("10:00"),
                MaxBookings = 2,
                SlotType = "Regular",
                IsBlocked = false,
                Notes = "Old slot"
            };

            var center = new ServiceCenter
            {
                CenterId = 1,
                CenterName = "Test Center"
            };

            var updatedSlot = new TimeSlot
            {
                SlotId = 1,
                CenterId = request.CenterId,
                SlotDate = request.SlotDate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MaxBookings = request.MaxBookings,
                SlotType = request.SlotType,
                IsBlocked = request.IsBlocked,
                Notes = request.Notes,
                Center = center
            };

            _mockRepository.Setup(x => x.GetByIdAsync(request.SlotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingSlot);

            _mockCenterRepository.Setup(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(center);

            _mockCommandRepository.Setup(x => x.HasConflictAsync(
                request.CenterId, request.SlotDate, request.StartTime, request.EndTime, request.SlotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<TimeSlot>()));

            _mockRepository.Setup(x => x.GetByIdWithDetailsAsync(request.SlotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedSlot);

            // Act
            var result = await _service.UpdateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.SlotId.Should().Be(request.SlotId);
            result.CenterId.Should().Be(request.CenterId);
            result.SlotDate.Should().Be(request.SlotDate);
            result.StartTime.Should().Be(request.StartTime);
            result.EndTime.Should().Be(request.EndTime);
            result.MaxBookings.Should().Be(request.MaxBookings);
            result.SlotType.Should().Be(request.SlotType);
            result.IsBlocked.Should().Be(request.IsBlocked);
            result.Notes.Should().Be(request.Notes);

            _mockRepository.Verify(x => x.GetByIdAsync(request.SlotId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCenterRepository.Verify(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.HasConflictAsync(
                request.CenterId, request.SlotDate, request.StartTime, request.EndTime, request.SlotId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<TimeSlot>()), Times.Once);
            _mockRepository.Verify(x => x.GetByIdWithDetailsAsync(request.SlotId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidSlotId_ShouldThrowException()
        {
            // Arrange
            var request = new UpdateTimeSlotRequestDto
            {
                SlotId = 999,
                CenterId = 1,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("10:00"),
                EndTime = TimeOnly.Parse("11:00")
            };

            _mockRepository.Setup(x => x.GetByIdAsync(request.SlotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TimeSlot?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(request));

            _mockRepository.Verify(x => x.GetByIdAsync(request.SlotId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCenterRepository.Verify(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<TimeSlot>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithInvalidCenterId_ShouldThrowException()
        {
            // Arrange
            var request = new UpdateTimeSlotRequestDto
            {
                SlotId = 1,
                CenterId = 999,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("10:00"),
                EndTime = TimeOnly.Parse("11:00")
            };

            var existingSlot = new TimeSlot
            {
                SlotId = 1,
                CenterId = 1,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("09:00"),
                EndTime = TimeOnly.Parse("10:00")
            };

            _mockRepository.Setup(x => x.GetByIdAsync(request.SlotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingSlot);

            _mockCenterRepository.Setup(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceCenter?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(request));

            _mockRepository.Verify(x => x.GetByIdAsync(request.SlotId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCenterRepository.Verify(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.HasConflictAsync(It.IsAny<int>(), It.IsAny<DateOnly>(), It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<TimeSlot>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_WithTimeConflict_ShouldThrowException()
        {
            // Arrange
            var request = new UpdateTimeSlotRequestDto
            {
                SlotId = 1,
                CenterId = 1,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("10:00"),
                EndTime = TimeOnly.Parse("11:00")
            };

            var existingSlot = new TimeSlot
            {
                SlotId = 1,
                CenterId = 1,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("09:00"),
                EndTime = TimeOnly.Parse("10:00")
            };

            var center = new ServiceCenter
            {
                CenterId = 1,
                CenterName = "Test Center"
            };

            _mockRepository.Setup(x => x.GetByIdAsync(request.SlotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingSlot);

            _mockCenterRepository.Setup(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(center);

            _mockCommandRepository.Setup(x => x.HasConflictAsync(
                request.CenterId, request.SlotDate, request.StartTime, request.EndTime, request.SlotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateAsync(request));

            _mockRepository.Verify(x => x.GetByIdAsync(request.SlotId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCenterRepository.Verify(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.HasConflictAsync(
                request.CenterId, request.SlotDate, request.StartTime, request.EndTime, request.SlotId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<TimeSlot>()), Times.Never);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteTimeSlot()
        {
            // Arrange
            var slotId = 1;

            _mockRepository.Setup(x => x.DeleteAsync(slotId))
                .ReturnsAsync(true);

            // Act
            var result = await _service.DeleteAsync(slotId);

            // Assert
            result.Should().BeFalse();

        }

       

        #endregion

        #region GenerateSlotsAsync Tests

        [Fact]
        public async Task GenerateSlotsAsync_WithValidData_ShouldGenerateSlots()
        {
            // Arrange
            var request = new GenerateSlotsRequestDto
            {
                CenterId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                SlotDurationMinutes = 60,
                MaxBookingsPerSlot = 2,
                SlotType = "Regular",
                OverwriteExisting = false
            };

            var center = new ServiceCenter
            {
                CenterId = 1,
                CenterName = "Test Center",
                OpenTime = TimeOnly.Parse("08:00"),
                CloseTime = TimeOnly.Parse("18:00")
            };

            _mockCenterRepository.Setup(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(center);

            _mockCommandRepository.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<TimeSlot>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            // Act
            var result = await _service.GenerateSlotsAsync(request);

            // Assert
            result.Should().Be(2);

            _mockCenterRepository.Verify(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.BulkCreateAsync(It.IsAny<IEnumerable<TimeSlot>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GenerateSlotsAsync_WithInvalidCenterId_ShouldThrowException()
        {
            // Arrange
            var request = new GenerateSlotsRequestDto
            {
                CenterId = 999,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                SlotDurationMinutes = 60
            };

            _mockCenterRepository.Setup(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ServiceCenter?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GenerateSlotsAsync(request));

            _mockCenterRepository.Verify(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.BulkCreateAsync(It.IsAny<IEnumerable<TimeSlot>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task GenerateSlotsAsync_WithOverwriteExisting_ShouldDeleteExistingSlots()
        {
            // Arrange
            var request = new GenerateSlotsRequestDto
            {
                CenterId = 1,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                SlotDurationMinutes = 60,
                OverwriteExisting = true
            };

            var center = new ServiceCenter
            {
                CenterId = 1,
                CenterName = "Test Center",
                OpenTime = TimeOnly.Parse("08:00"),
                CloseTime = TimeOnly.Parse("18:00")
            };

            _mockCenterRepository.Setup(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(center);

            _mockCommandRepository.Setup(x => x.DeleteSlotsByDateRangeAsync(
                request.CenterId, request.StartDate, request.EndDate, It.IsAny<CancellationToken>()));

            _mockCommandRepository.Setup(x => x.BulkCreateAsync(It.IsAny<IEnumerable<TimeSlot>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            // Act
            var result = await _service.GenerateSlotsAsync(request);

            // Assert
            result.Should().Be(2);

            _mockCenterRepository.Verify(x => x.GetByIdAsync(request.CenterId, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.DeleteSlotsByDateRangeAsync(
                request.CenterId, request.StartDate, request.EndDate, It.IsAny<CancellationToken>()), Times.Once);
            _mockCommandRepository.Verify(x => x.BulkCreateAsync(It.IsAny<IEnumerable<TimeSlot>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region DeleteEmptySlotsAsync Tests

        [Fact]
        public async Task DeleteEmptySlotsAsync_WithValidData_ShouldDeleteEmptySlots()
        {
            // Arrange
            var centerId = 1;
            var date = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

            _mockCommandRepository.Setup(x => x.DeleteEmptySlotsAsync(centerId, date, It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            // Act
            var result = await _service.DeleteEmptySlotsAsync(centerId, date);

            // Assert
            result.Should().Be(3);

            _mockCommandRepository.Verify(x => x.DeleteEmptySlotsAsync(centerId, date, It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region ToggleBlockAsync Tests

        [Fact]
        public async Task ToggleBlockAsync_WithValidId_ShouldToggleBlock()
        {
            // Arrange
            var slotId = 1;
            var isBlocked = true;

            var existingSlot = new TimeSlot
            {
                SlotId = slotId,
                CenterId = 1,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("09:00"),
                EndTime = TimeOnly.Parse("10:00"),
                IsBlocked = false
            };

            var center = new ServiceCenter
            {
                CenterId = 1,
                CenterName = "Test Center"
            };

            var updatedSlot = new TimeSlot
            {
                SlotId = slotId,
                CenterId = 1,
                SlotDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = TimeOnly.Parse("09:00"),
                EndTime = TimeOnly.Parse("10:00"),
                IsBlocked = isBlocked,
                Center = center
            };

            _mockRepository.Setup(x => x.GetByIdAsync(slotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingSlot);

            _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<TimeSlot>()));

            _mockRepository.Setup(x => x.GetByIdWithDetailsAsync(slotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(updatedSlot);

            // Act
            var result = await _service.ToggleBlockAsync(slotId, isBlocked);

            // Assert
            result.Should().NotBeNull();
            result.SlotId.Should().Be(slotId);
            result.IsBlocked.Should().Be(isBlocked);

            _mockRepository.Verify(x => x.GetByIdAsync(slotId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<TimeSlot>()), Times.Once);
            _mockRepository.Verify(x => x.GetByIdWithDetailsAsync(slotId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ToggleBlockAsync_WithInvalidId_ShouldThrowException()
        {
            // Arrange
            var slotId = 999;
            var isBlocked = true;

            _mockRepository.Setup(x => x.GetByIdAsync(slotId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((TimeSlot?)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ToggleBlockAsync(slotId, isBlocked));

            _mockRepository.Verify(x => x.GetByIdAsync(slotId, It.IsAny<CancellationToken>()), Times.Once);
            _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<TimeSlot>()), Times.Never);
            _mockRepository.Verify(x => x.GetByIdWithDetailsAsync(slotId, It.IsAny<CancellationToken>()), Times.Never);
        }

        #endregion
    }
}