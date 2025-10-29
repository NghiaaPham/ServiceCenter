using FluentAssertions;
using EVServiceCenter.Core.Helpers;
using EVServiceCenter.Core.Enums;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Helpers;

/// <summary>
/// Unit tests for AppointmentStatusHelper class
/// </summary>
public class AppointmentStatusHelperTests
{
    [Theory]
    [InlineData(AppointmentStatusEnum.Cancelled)]
    [InlineData(AppointmentStatusEnum.NoShow)]
    [InlineData(AppointmentStatusEnum.Rescheduled)]
    public void ShouldExcludeFromCapacity_WithExcludedStatuses_ShouldReturnTrue(AppointmentStatusEnum status)
    {
        // Act
        var result = AppointmentStatusHelper.ShouldExcludeFromCapacity((int)status);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(AppointmentStatusEnum.Pending)]
    [InlineData(AppointmentStatusEnum.Confirmed)]
    [InlineData(AppointmentStatusEnum.CheckedIn)]
    [InlineData(AppointmentStatusEnum.InProgress)]
    [InlineData(AppointmentStatusEnum.Completed)]
    public void ShouldExcludeFromCapacity_WithActiveStatuses_ShouldReturnFalse(AppointmentStatusEnum status)
    {
        // Act
        var result = AppointmentStatusHelper.ShouldExcludeFromCapacity((int)status);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(AppointmentStatusEnum.Pending)]
    [InlineData(AppointmentStatusEnum.Confirmed)]
    [InlineData(AppointmentStatusEnum.CheckedIn)]
    [InlineData(AppointmentStatusEnum.InProgress)]
    [InlineData(AppointmentStatusEnum.Completed)]
    public void IsActiveBooking_WithActiveStatuses_ShouldReturnTrue(AppointmentStatusEnum status)
    {
        // Act
        var result = AppointmentStatusHelper.IsActiveBooking((int)status);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(AppointmentStatusEnum.Cancelled)]
    [InlineData(AppointmentStatusEnum.NoShow)]
    [InlineData(AppointmentStatusEnum.Rescheduled)]
    public void IsActiveBooking_WithExcludedStatuses_ShouldReturnFalse(AppointmentStatusEnum status)
    {
        // Act
        var result = AppointmentStatusHelper.IsActiveBooking((int)status);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(AppointmentStatusEnum.Completed)]
    [InlineData(AppointmentStatusEnum.Cancelled)]
    [InlineData(AppointmentStatusEnum.Rescheduled)]
    [InlineData(AppointmentStatusEnum.NoShow)]
    public void IsFinalStatus_WithFinalStatuses_ShouldReturnTrue(AppointmentStatusEnum status)
    {
        // Act
        var result = AppointmentStatusHelper.IsFinalStatus((int)status);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(AppointmentStatusEnum.Pending)]
    [InlineData(AppointmentStatusEnum.Confirmed)]
    [InlineData(AppointmentStatusEnum.CheckedIn)]
    [InlineData(AppointmentStatusEnum.InProgress)]
    public void IsFinalStatus_WithNonFinalStatuses_ShouldReturnFalse(AppointmentStatusEnum status)
    {
        // Act
        var result = AppointmentStatusHelper.IsFinalStatus((int)status);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ExcludedFromCapacity_ShouldContainCorrectStatuses()
    {
        // Arrange & Act
        var excludedStatuses = AppointmentStatusHelper.ExcludedFromCapacity;

        // Assert
        excludedStatuses.Should().Contain((int)AppointmentStatusEnum.Cancelled);
        excludedStatuses.Should().Contain((int)AppointmentStatusEnum.NoShow);
        excludedStatuses.Should().Contain((int)AppointmentStatusEnum.Rescheduled);
        excludedStatuses.Should().HaveCount(3);
    }

    [Fact]
    public void ActiveBookings_ShouldContainCorrectStatuses()
    {
        // Arrange & Act
        var activeBookings = AppointmentStatusHelper.ActiveBookings;

        // Assert
        activeBookings.Should().Contain((int)AppointmentStatusEnum.Pending);
        activeBookings.Should().Contain((int)AppointmentStatusEnum.Confirmed);
        activeBookings.Should().Contain((int)AppointmentStatusEnum.CheckedIn);
        activeBookings.Should().Contain((int)AppointmentStatusEnum.InProgress);
        activeBookings.Should().Contain((int)AppointmentStatusEnum.Completed);
        activeBookings.Should().HaveCount(5);
    }

    [Fact]
    public void FinalStatuses_ShouldContainCorrectStatuses()
    {
        // Arrange & Act
        var finalStatuses = AppointmentStatusHelper.FinalStatuses;

        // Assert
        finalStatuses.Should().Contain((int)AppointmentStatusEnum.Completed);
        finalStatuses.Should().Contain((int)AppointmentStatusEnum.Cancelled);
        finalStatuses.Should().Contain((int)AppointmentStatusEnum.Rescheduled);
        finalStatuses.Should().Contain((int)AppointmentStatusEnum.NoShow);
        finalStatuses.Should().HaveCount(4);
    }

    [Theory]
    [InlineData(999)]
    [InlineData(-1)]
    [InlineData(100)]
    public void IsActiveBooking_WithInvalidStatusId_ShouldReturnFalse(int statusId)
    {
        // Act
        var result = AppointmentStatusHelper.IsActiveBooking(statusId);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(999)]
    [InlineData(-1)]
    [InlineData(100)]
    public void ShouldExcludeFromCapacity_WithInvalidStatusId_ShouldReturnFalse(int statusId)
    {
        // Act
        var result = AppointmentStatusHelper.ShouldExcludeFromCapacity(statusId);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(999)]
    [InlineData(-1)]
    [InlineData(100)]
    public void IsFinalStatus_WithInvalidStatusId_ShouldReturnFalse(int statusId)
    {
        // Act
        var result = AppointmentStatusHelper.IsFinalStatus(statusId);

        // Assert
        result.Should().BeFalse();
    }
}
