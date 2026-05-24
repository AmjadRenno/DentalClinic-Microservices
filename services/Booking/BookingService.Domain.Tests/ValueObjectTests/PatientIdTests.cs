using BookingService.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BookingService.Domain.Tests.ValueObjectTests;

public class PatientIdTests
{
    [Fact]
    public void Constructor_WithValidGuid_ShouldCreatePatientId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var patientId = new PatientId(guid);

        // Assert
        patientId.Value.Should().Be(guid);
    }

    [Fact]
    public void PatientIds_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = new PatientId(guid);
        var id2 = new PatientId(guid);

        // Act & Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void PatientIds_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = new PatientId(Guid.NewGuid());
        var id2 = new PatientId(Guid.NewGuid());

        // Act & Assert
        id1.Should().NotBe(id2);
        (id1 != id2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var patientId = new PatientId(guid);

        // Act
        var result = patientId.ToString();

        // Assert - Record types include property names in ToString()
        result.Should().Contain(guid.ToString());
    }
}
