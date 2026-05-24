using BookingService.Domain.Entities;
using BookingService.Domain.ValueObjects;

namespace BookingService.Domain.Tests.TestHelpers;

/// <summary>
/// Builder pattern for creating test Appointment instances
/// </summary>
public static class AppointmentBuilder
{
    public static Appointment Create()
    {
        return new Appointment(
            Guid.NewGuid(),
            new PatientId(Guid.NewGuid()),
            new DentistId(Guid.NewGuid()),
            CreateValidTimeSlot());
    }

    public static Appointment CreateWithId(Guid id)
    {
        return new Appointment(
            id,
            new PatientId(Guid.NewGuid()),
            new DentistId(Guid.NewGuid()),
            CreateValidTimeSlot());
    }

    public static Appointment CreateWithPatient(Guid patientId)
    {
        return new Appointment(
            Guid.NewGuid(),
            new PatientId(patientId),
            new DentistId(Guid.NewGuid()),
            CreateValidTimeSlot());
    }

    public static Appointment CreateWithDentist(Guid dentistId)
    {
        return new Appointment(
            Guid.NewGuid(),
            new PatientId(Guid.NewGuid()),
            new DentistId(dentistId),
            CreateValidTimeSlot());
    }

    public static Appointment CreateWithTimeSlot(TimeSlot slot)
    {
        return new Appointment(
            Guid.NewGuid(),
            new PatientId(Guid.NewGuid()),
            new DentistId(Guid.NewGuid()),
            slot);
    }

    public static Appointment CreateConfirmed()
    {
        var appointment = Create();
        appointment.Confirm();
        return appointment;
    }

    public static Appointment CreateCancelled()
    {
        var appointment = Create();
        appointment.Cancel();
        return appointment;
    }

    public static Appointment CreateCompleted()
    {
        var appointment = Create();
        appointment.Confirm();
        appointment.Complete();
        return appointment;
    }

    public static TimeSlot CreateValidTimeSlot(int daysFromNow = 1, int startHour = 10)
    {
        var start = DateTime.UtcNow.AddDays(daysFromNow).Date.AddHours(startHour);
        var end = start.AddHours(1);
        return new TimeSlot(start, end);
    }

    public static TimeSlot CreateTimeSlot(DateTime start, DateTime end)
    {
        return new TimeSlot(start, end);
    }
}
