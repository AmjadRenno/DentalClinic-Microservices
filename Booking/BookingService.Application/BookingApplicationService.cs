using BookingService.Application.Commands;
using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Domain.ValueObjects;
using Dapr.Client;
using DentalClinic.SharedKernel.DomainEvents;

namespace BookingService.Application;

public class BookingApplicationService
{
    private readonly IAppointmentRepository _repository;
    private readonly DaprClient _daprClient;

    public BookingApplicationService(IAppointmentRepository repository, DaprClient daprClient)
    {
        _repository = repository;
        _daprClient = daprClient;
    }

    public async Task Handle(RequestAppointmentCommand command)
    {
        var appointment = new Appointment(
            command.AppointmentId,
            new PatientId(command.PatientId),
            new DentistId(command.DentistId),
            new TimeSlot(command.Start, command.End));

        await _repository.AddAsync(appointment);
    }

    public async Task Handle(ConfirmAppointmentCommand command)
    {
        var appointment = await _repository.GetByIdAsync(command.AppointmentId)
            ?? throw new InvalidOperationException("Appointment not found.");

        appointment.Confirm();
        await _repository.UpdateAsync(appointment);

        // ✅ نشر الحدث عبر Dapr Pub/Sub
        var evt = new AppointmentConfirmedEvent(appointment.Id, appointment.PatientId.Value);
        await _daprClient.PublishEventAsync("pubsub", "appointments.confirmed", evt);
        Console.WriteLine($"📤 Published event to pubsub: appointments.confirmed ({evt.AppointmentId})");
    }

    public async Task Handle(CancelAppointmentCommand command)
    {
        var appointment = await _repository.GetByIdAsync(command.AppointmentId)
            ?? throw new InvalidOperationException("Appointment not found.");

        appointment.Cancel();
        await _repository.UpdateAsync(appointment);
    }

    public async Task Handle(RescheduleAppointmentCommand command)
    {
        var appointment = await _repository.GetByIdAsync(command.AppointmentId)
            ?? throw new InvalidOperationException("Appointment not found.");

        appointment.Reschedule(new TimeSlot(command.NewStart, command.NewEnd));
        await _repository.UpdateAsync(appointment);
    }
}
