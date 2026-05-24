using BookingService.Domain.Entities;
using BookingService.Domain.ValueObjects;
using BookingService.Domain.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace BookingService.Domain.Tests.EntityTests;

public class AppointmentTests
{
    [Fact]
    public void Constructor_ShouldCreateAppointment_WithRequestedStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var patientId = new PatientId(Guid.NewGuid());
        var dentistId = new DentistId(Guid.NewGuid());
        var slot = AppointmentBuilder.CreateValidTimeSlot();

        // Act
        var appointment = new Appointment(id, patientId, dentistId, slot);

        // Assert
        appointment.Id.Should().Be(id);
        appointment.PatientId.Should().Be(patientId);
        appointment.DentistId.Should().Be(dentistId);
        appointment.Slot.Should().Be(slot);
        appointment.Status.Should().Be(AppointmentStatus.Requested);
    }

    [Fact]
    public void Appointments_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var appointment1 = AppointmentBuilder.CreateWithId(id);
        var appointment2 = AppointmentBuilder.CreateWithId(id);

        // Act & Assert
        appointment1.Should().Be(appointment2);
        (appointment1 == appointment2).Should().BeTrue();
    }

    [Fact]
    public void Appointments_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var appointment1 = AppointmentBuilder.Create();
        var appointment2 = AppointmentBuilder.Create();

        // Act & Assert
        appointment1.Should().NotBe(appointment2);
        (appointment1 != appointment2).Should().BeTrue();
    }

    [Fact]
    public void Confirm_WhenStatusIsRequested_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var appointment = AppointmentBuilder.Create();

        // Act
        appointment.Confirm();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenStatusIsNotRequested_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = AppointmentBuilder.Create();
        appointment.Confirm(); // Already confirmed

        // Act
        var act = () => appointment.Confirm();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only requested appointments can be confirmed.");
    }

    [Fact]
    public void Cancel_WhenStatusIsRequested_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var appointment = AppointmentBuilder.Create();

        // Act
        appointment.Cancel();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenStatusIsConfirmed_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var appointment = AppointmentBuilder.Create();
        appointment.Confirm();

        // Act
        appointment.Cancel();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenStatusIsCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = AppointmentBuilder.CreateCompleted();

        // Act
        var act = () => appointment.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Completed appointments cannot be cancelled.");
    }

    [Fact]
    public void Complete_WhenStatusIsConfirmed_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var appointment = AppointmentBuilder.CreateConfirmed();

        // Act
        appointment.Complete();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void Complete_WhenStatusIsRequested_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = AppointmentBuilder.Create();

        // Act
        var act = () => appointment.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only confirmed appointments can be completed.");
    }

    [Fact]
    public void Complete_WhenStatusIsCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = AppointmentBuilder.CreateCancelled();

        // Act
        var act = () => appointment.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only confirmed appointments can be completed.");
    }

    [Fact]
    public void Reschedule_WhenStatusIsRequested_ShouldUpdateTimeSlot()
    {
        // Arrange
        var appointment = AppointmentBuilder.Create();
        var newSlot = AppointmentBuilder.CreateValidTimeSlot(daysFromNow: 2);

        // Act
        appointment.Reschedule(newSlot);

        // Assert
        appointment.Slot.Should().Be(newSlot);
    }

    [Fact]
    public void Reschedule_WhenStatusIsConfirmed_ShouldUpdateTimeSlot()
    {
        // Arrange
        var appointment = AppointmentBuilder.CreateConfirmed();
        var newSlot = AppointmentBuilder.CreateValidTimeSlot(daysFromNow: 3);

        // Act
        appointment.Reschedule(newSlot);

        // Assert
        appointment.Slot.Should().Be(newSlot);
    }

    [Fact]
    public void Reschedule_WhenStatusIsCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var appointment = AppointmentBuilder.CreateCancelled();
        var newSlot = AppointmentBuilder.CreateValidTimeSlot(daysFromNow: 1);

        // Act
        var act = () => appointment.Reschedule(newSlot);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cancelled appointments cannot be rescheduled.");
    }

    [Theory]
    [InlineData(AppointmentStatus.Requested)]
    [InlineData(AppointmentStatus.Confirmed)]
    public void Reschedule_WithValidStatuses_ShouldSucceed(AppointmentStatus initialStatus)
    {
        // Arrange
        var appointment = initialStatus == AppointmentStatus.Requested
            ? AppointmentBuilder.Create()
            : AppointmentBuilder.CreateConfirmed();
        
        var newSlot = AppointmentBuilder.CreateValidTimeSlot(daysFromNow: 5);

        // Act
        appointment.Reschedule(newSlot);

        // Assert
        appointment.Slot.Should().Be(newSlot);
        appointment.Status.Should().Be(initialStatus); // Status should not change
    }

    [Fact]
    public void AppointmentLifecycle_FromRequestedToCompleted_ShouldWorkCorrectly()
    {
        // Arrange
        var appointment = AppointmentBuilder.Create();

        // Act & Assert - Step by step lifecycle
        appointment.Status.Should().Be(AppointmentStatus.Requested);
        
        appointment.Confirm();
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        
        appointment.Complete();
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void AppointmentLifecycle_RequestedToCancelled_ShouldWorkCorrectly()
    {
        // Arrange
        var appointment = AppointmentBuilder.Create();

        // Act
        appointment.Cancel();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public void AppointmentLifecycle_ConfirmedToCancelled_ShouldWorkCorrectly()
    {
        // Arrange
        var appointment = AppointmentBuilder.Create();
        appointment.Confirm();

        // Act
        appointment.Cancel();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }
}
