using BookingService.Domain.Entities;
using BookingService.Domain.ValueObjects;
using Xunit;

namespace BookingService.Domain.Tests;

public class AppointmentTests
{
    [Fact]
    public void Appointments_with_same_id_should_be_equal()
    {
        // Arrange
        var id = Guid.NewGuid();
        var patientId = new PatientId(Guid.NewGuid());
        var dentistId = new DentistId(Guid.NewGuid());
        var slot = new TimeSlot(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2));

        var a1 = new Appointment(id, patientId, dentistId, slot);
        var a2 = new Appointment(id, patientId, dentistId, slot);

        // Act + Assert
        Assert.True(a1.Equals(a2));
        Assert.True(a1 == a2);
    }

    [Fact]
    public void Confirm_flow_should_change_status_correctly()
    {
        var appointment = new Appointment(
            Guid.NewGuid(),
            new PatientId(Guid.NewGuid()),
            new DentistId(Guid.NewGuid()),
            new TimeSlot(DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2)));

        appointment.Confirm();
        appointment.Complete();

        Assert.Equal(AppointmentStatus.Completed, appointment.Status);
    }

    [Fact]
    public void TimeSlot_should_throw_if_end_before_start()
    {
        var start = DateTime.UtcNow.AddHours(2);
        var end = DateTime.UtcNow.AddHours(1);

        Assert.Throws<ArgumentException>(() => new TimeSlot(start, end));
    }
}