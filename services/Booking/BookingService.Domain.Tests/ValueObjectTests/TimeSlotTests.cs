using BookingService.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BookingService.Domain.Tests.ValueObjectTests;

public class TimeSlotTests
{
    [Fact]
    public void Constructor_WithValidDates_ShouldCreateTimeSlot()
    {
        // Arrange
        var start = DateTime.UtcNow.AddHours(1);
        var end = start.AddHours(1);

        // Act
        var slot = new TimeSlot(start, end);

        // Assert
        slot.Start.Should().Be(start);
        slot.End.Should().Be(end);
    }

    [Fact]
    public void Constructor_WhenEndBeforeStart_ShouldThrowArgumentException()
    {
        // Arrange
        var start = DateTime.UtcNow.AddHours(2);
        var end = DateTime.UtcNow.AddHours(1);

        // Act
        var act = () => new TimeSlot(start, end);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("End time must be after start time.*")
            .WithParameterName("end");
    }

    [Fact]
    public void Constructor_WhenEndEqualsStart_ShouldThrowArgumentException()
    {
        // Arrange
        var start = DateTime.UtcNow.AddHours(1);
        var end = start;

        // Act
        var act = () => new TimeSlot(start, end);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("End time must be after start time.*");
    }

    [Fact]
    public void Duration_ShouldReturnCorrectTimeSpan()
    {
        // Arrange
        var start = DateTime.UtcNow;
        var end = start.AddHours(2).AddMinutes(30);
        var slot = new TimeSlot(start, end);

        // Act
        var duration = slot.Duration;

        // Assert
        duration.Should().Be(TimeSpan.FromHours(2.5));
    }

    [Theory]
    [InlineData(1, 60)] // 1 hour
    [InlineData(0.5, 30)] // 30 minutes
    [InlineData(2, 120)] // 2 hours
    [InlineData(24, 1440)] // 1 day
    public void Duration_WithVariousTimeSpans_ShouldCalculateCorrectly(double hours, int expectedMinutes)
    {
        // Arrange
        var start = DateTime.UtcNow;
        var end = start.AddHours(hours);
        var slot = new TimeSlot(start, end);

        // Act
        var duration = slot.Duration;

        // Assert
        duration.TotalMinutes.Should().Be(expectedMinutes);
    }

    [Fact]
    public void TimeSlots_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var start = DateTime.UtcNow;
        var end = start.AddHours(1);
        var slot1 = new TimeSlot(start, end);
        var slot2 = new TimeSlot(start, end);

        // Act & Assert
        slot1.Should().Be(slot2);
        (slot1 == slot2).Should().BeTrue();
    }

    [Fact]
    public void TimeSlots_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var start = DateTime.UtcNow;
        var slot1 = new TimeSlot(start, start.AddHours(1));
        var slot2 = new TimeSlot(start, start.AddHours(2));

        // Act & Assert
        slot1.Should().NotBe(slot2);
        (slot1 != slot2).Should().BeTrue();
    }

    [Fact]
    public void GetHashCode_ForEqualTimeSlots_ShouldBeSame()
    {
        // Arrange
        var start = DateTime.UtcNow;
        var end = start.AddHours(1);
        var slot1 = new TimeSlot(start, end);
        var slot2 = new TimeSlot(start, end);

        // Act & Assert
        slot1.GetHashCode().Should().Be(slot2.GetHashCode());
    }
}
