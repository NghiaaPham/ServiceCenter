using FluentAssertions;
using EVServiceCenter.Core.Helpers;
using EVServiceCenter.Core.Domains.TimeSlots.Entities;
using Xunit;

namespace EVServiceCenter.Tests.Unit.Helpers;

/// <summary>
/// Unit tests for TimeSlotHelper class
/// </summary>
public class TimeSlotHelperTests
{
    [Fact]
    public void DoTimesOverlap_WithOverlappingTimes_ShouldReturnTrue()
    {
        // Arrange
        var start1 = new DateTime(2024, 1, 1, 9, 0, 0);
        var end1 = new DateTime(2024, 1, 1, 11, 0, 0);
        var start2 = new DateTime(2024, 1, 1, 10, 0, 0);
        var end2 = new DateTime(2024, 1, 1, 12, 0, 0);

        // Act
        var result = TimeSlotHelper.DoTimesOverlap(start1, end1, start2, end2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DoTimesOverlap_WithNonOverlappingTimes_ShouldReturnFalse()
    {
        // Arrange
        var start1 = new DateTime(2024, 1, 1, 9, 0, 0);
        var end1 = new DateTime(2024, 1, 1, 10, 0, 0);
        var start2 = new DateTime(2024, 1, 1, 11, 0, 0);
        var end2 = new DateTime(2024, 1, 1, 12, 0, 0);

        // Act
        var result = TimeSlotHelper.DoTimesOverlap(start1, end1, start2, end2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DoTimesOverlap_WithAdjacentTimes_ShouldReturnFalse()
    {
        // Arrange
        var start1 = new DateTime(2024, 1, 1, 9, 0, 0);
        var end1 = new DateTime(2024, 1, 1, 10, 0, 0);
        var start2 = new DateTime(2024, 1, 1, 10, 0, 0);
        var end2 = new DateTime(2024, 1, 1, 11, 0, 0);

        // Act
        var result = TimeSlotHelper.DoTimesOverlap(start1, end1, start2, end2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DoTimesOverlap_WithSameTimes_ShouldReturnTrue()
    {
        // Arrange
        var start1 = new DateTime(2024, 1, 1, 9, 0, 0);
        var end1 = new DateTime(2024, 1, 1, 10, 0, 0);
        var start2 = new DateTime(2024, 1, 1, 9, 0, 0);
        var end2 = new DateTime(2024, 1, 1, 10, 0, 0);

        // Act
        var result = TimeSlotHelper.DoTimesOverlap(start1, end1, start2, end2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DoSlotsOverlap_WithSameDateAndOverlappingTimes_ShouldReturnTrue()
    {
        // Arrange
        var date = new DateOnly(2024, 1, 1);
        var slot1 = new TimeSlot
        {
            SlotDate = date,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(11, 0)
        };
        var slot2 = new TimeSlot
        {
            SlotDate = date,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(12, 0)
        };

        // Act
        var result = TimeSlotHelper.DoSlotsOverlap(slot1, slot2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DoSlotsOverlap_WithDifferentDates_ShouldReturnFalse()
    {
        // Arrange
        var slot1 = new TimeSlot
        {
            SlotDate = new DateOnly(2024, 1, 1),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(11, 0)
        };
        var slot2 = new TimeSlot
        {
            SlotDate = new DateOnly(2024, 1, 2),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(11, 0)
        };

        // Act
        var result = TimeSlotHelper.DoSlotsOverlap(slot1, slot2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AreSlotsAdjacent_WithAdjacentTimes_ShouldReturnTrue()
    {
        // Arrange
        var date = new DateOnly(2024, 1, 1);
        var slot1 = new TimeSlot
        {
            SlotDate = date,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };
        var slot2 = new TimeSlot
        {
            SlotDate = date,
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0)
        };

        // Act
        var result = TimeSlotHelper.AreSlotsAdjacent(slot1, slot2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AreSlotsAdjacent_WithDifferentDates_ShouldReturnFalse()
    {
        // Arrange
        var slot1 = new TimeSlot
        {
            SlotDate = new DateOnly(2024, 1, 1),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(10, 0)
        };
        var slot2 = new TimeSlot
        {
            SlotDate = new DateOnly(2024, 1, 2),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(11, 0)
        };

        // Act
        var result = TimeSlotHelper.AreSlotsAdjacent(slot1, slot2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetSlotStartDateTime_ShouldReturnCorrectDateTime()
    {
        // Arrange
        var slot = new TimeSlot
        {
            SlotDate = new DateOnly(2024, 1, 15),
            StartTime = new TimeOnly(9, 30)
        };

        // Act
        var result = TimeSlotHelper.GetSlotStartDateTime(slot);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 15, 9, 30, 0));
    }

    [Fact]
    public void GetSlotEndDateTime_ShouldReturnCorrectDateTime()
    {
        // Arrange
        var slot = new TimeSlot
        {
            SlotDate = new DateOnly(2024, 1, 15),
            EndTime = new TimeOnly(11, 45)
        };

        // Act
        var result = TimeSlotHelper.GetSlotEndDateTime(slot);

        // Assert
        result.Should().Be(new DateTime(2024, 1, 15, 11, 45, 0));
    }

    [Fact]
    public void IsSameDay_WithSameDate_ShouldReturnTrue()
    {
        // Arrange
        var slot = new TimeSlot
        {
            SlotDate = new DateOnly(2024, 1, 15)
        };
        var date = new DateOnly(2024, 1, 15);

        // Act
        var result = TimeSlotHelper.IsSameDay(slot, date);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSameDay_WithDifferentDate_ShouldReturnFalse()
    {
        // Arrange
        var slot = new TimeSlot
        {
            SlotDate = new DateOnly(2024, 1, 15)
        };
        var date = new DateOnly(2024, 1, 16);

        // Act
        var result = TimeSlotHelper.IsSameDay(slot, date);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSlotInPast_WithPastSlot_ShouldReturnTrue()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddDays(-1);
        var slot = new TimeSlot
        {
            SlotDate = DateOnly.FromDateTime(pastDate),
            StartTime = TimeOnly.FromDateTime(pastDate)
        };

        // Act
        var result = TimeSlotHelper.IsSlotInPast(slot);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSlotInPast_WithFutureSlot_ShouldReturnFalse()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var slot = new TimeSlot
        {
            SlotDate = DateOnly.FromDateTime(futureDate),
            StartTime = TimeOnly.FromDateTime(futureDate)
        };

        // Act
        var result = TimeSlotHelper.IsSlotInPast(slot);

        // Assert
        result.Should().BeFalse();
    }
}
