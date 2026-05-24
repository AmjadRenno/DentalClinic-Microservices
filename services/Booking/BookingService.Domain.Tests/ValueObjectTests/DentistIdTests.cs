using BookingService.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BookingService.Domain.Tests.ValueObjectTests;

public class DentistIdTests
{
    [Fact]
    public void Constructor_WithValidGuid_ShouldCreateDentistId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var dentistId = new DentistId(guid);

        // Assert
        dentistId.Value.Should().Be(guid);
    }

    [Fact]
    public void DentistIds_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new DentistId(guid);
        var id2 = new DentistId(guid);

        // Act & Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void DentistIds_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = new DentistId(Guid.NewGuid());
        var id2 = new DentistId(Guid.NewGuid());

        // Act & Assert
        id1.Should().NotBe(id2);
        (id1 != id2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var dentistId = new DentistId(guid);

        // Act
        var result = dentistId.ToString();

        // Assert - Record types include property names in ToString()
        result.Should().Contain(guid.ToString());
    }
}
