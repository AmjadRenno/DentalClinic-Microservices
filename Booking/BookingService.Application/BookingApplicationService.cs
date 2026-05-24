using BookingService.Application.Commands;
using BookingService.Application.Interfaces;
using BookingService.Domain.Entities;
using BookingService.Domain.ValueObjects;
using Dapr.Client;
using DentalClinic.SharedKernel.DomainEvents;
using DentalClinic.SharedKernel.Exceptions;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace BookingService.Application;

public class BookingApplicationService
{
    private readonly IAppointmentRepository _repository;
    private readonly DaprClient _daprClient;
    private readonly IValidator<RequestAppointmentCommand> _requestValidator;
    private readonly IValidator<ConfirmAppointmentCommand> _confirmValidator;
    private readonly IValidator<CancelAppointmentCommand> _cancelValidator;
    private readonly IValidator<RescheduleAppointmentCommand> _rescheduleValidator;
    private readonly ILogger<BookingApplicationService> _logger;

    public BookingApplicationService(
        IAppointmentRepository repository,
        DaprClient daprClient,
        IValidator<RequestAppointmentCommand> requestValidator,
        IValidator<ConfirmAppointmentCommand> confirmValidator,
        IValidator<CancelAppointmentCommand> cancelValidator,
        IValidator<RescheduleAppointmentCommand> rescheduleValidator,
        ILogger<BookingApplicationService> logger)
    {
        _repository = repository;
        _daprClient = daprClient;
        _requestValidator = requestValidator;
        _confirmValidator = confirmValidator;
        _cancelValidator = cancelValidator;
        _rescheduleValidator = rescheduleValidator;
        _logger = logger;
    }

    public async Task Handle(RequestAppointmentCommand command)
    {
        // Validate command
        var validationResult = await _requestValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            
            throw new DentalClinic.SharedKernel.Exceptions.ValidationException(errors);
        }

        _logger.LogInformation(
            "Creating appointment {AppointmentId} for patient {PatientId}",
            command.AppointmentId,
            command.PatientId);

        try
        {
            var appointment = new Appointment(
                command.AppointmentId,
                new PatientId(command.PatientId),
                new DentistId(command.DentistId),
                new TimeSlot(command.Start, command.End));

            await _repository.AddAsync(appointment);

            _logger.LogInformation(
                "Successfully created appointment {AppointmentId}",
                command.AppointmentId);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument when creating appointment");
            throw new DentalClinic.SharedKernel.Exceptions.ValidationException(ex.Message);
        }
    }

    public async Task Handle(ConfirmAppointmentCommand command)
    {
        // Validate command
        var validationResult = await _confirmValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            
            throw new DentalClinic.SharedKernel.Exceptions.ValidationException(errors);
        }

        _logger.LogInformation("Confirming appointment {AppointmentId}", command.AppointmentId);

        var appointment = await _repository.GetByIdAsync(command.AppointmentId)
            ?? throw new NotFoundException("Appointment", command.AppointmentId);

        try
        {
            appointment.Confirm();
            await _repository.UpdateAsync(appointment);

            // Publish event via Dapr Pub/Sub
            var evt = new AppointmentConfirmedEvent(appointment.Id, appointment.PatientId.Value);
            await _daprClient.PublishEventAsync("pubsub", "appointments.confirmed", evt);

            _logger.LogInformation(
                "Successfully confirmed appointment {AppointmentId}",
                command.AppointmentId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot confirm appointment {AppointmentId}", command.AppointmentId);
            throw new BusinessRuleException(ex.Message, "CANNOT_CONFIRM_APPOINTMENT");
        }
    }

    public async Task Handle(CancelAppointmentCommand command)
    {
        // Validate command
        var validationResult = await _cancelValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            
            throw new DentalClinic.SharedKernel.Exceptions.ValidationException(errors);
        }

        _logger.LogInformation("Cancelling appointment {AppointmentId}", command.AppointmentId);

        var appointment = await _repository.GetByIdAsync(command.AppointmentId)
            ?? throw new NotFoundException("Appointment", command.AppointmentId);

        try
        {
            appointment.Cancel();
            await _repository.UpdateAsync(appointment);

            _logger.LogInformation(
                "Successfully cancelled appointment {AppointmentId}",
                command.AppointmentId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot cancel appointment {AppointmentId}", command.AppointmentId);
            throw new BusinessRuleException(ex.Message, "CANNOT_CANCEL_APPOINTMENT");
        }
    }

    public async Task Handle(RescheduleAppointmentCommand command)
    {
        // Validate command
        var validationResult = await _rescheduleValidator.ValidateAsync(command);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            
            throw new DentalClinic.SharedKernel.Exceptions.ValidationException(errors);
        }

        _logger.LogInformation("Rescheduling appointment {AppointmentId}", command.AppointmentId);

        var appointment = await _repository.GetByIdAsync(command.AppointmentId)
            ?? throw new NotFoundException("Appointment", command.AppointmentId);

        try
        {
            appointment.Reschedule(new TimeSlot(command.NewStart, command.NewEnd));
            await _repository.UpdateAsync(appointment);

            _logger.LogInformation(
                "Successfully rescheduled appointment {AppointmentId}",
                command.AppointmentId);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot reschedule appointment {AppointmentId}", command.AppointmentId);
            throw new BusinessRuleException(ex.Message, "CANNOT_RESCHEDULE_APPOINTMENT");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid time slot for rescheduling");
            throw new DentalClinic.SharedKernel.Exceptions.ValidationException(ex.Message);
        }
    }
}
