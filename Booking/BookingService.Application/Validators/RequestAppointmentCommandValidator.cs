using BookingService.Application.Commands;
using FluentValidation;

namespace BookingService.Application.Validators;

public class RequestAppointmentCommandValidator : AbstractValidator<RequestAppointmentCommand>
{
    public RequestAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("Appointment ID is required.");

        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithMessage("Patient ID is required.");

        RuleFor(x => x.DentistId)
            .NotEmpty()
            .WithMessage("Dentist ID is required.");

        RuleFor(x => x.Start)
            .NotEmpty()
            .WithMessage("Start time is required.")
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Appointment must be scheduled in the future.");

        RuleFor(x => x.End)
            .NotEmpty()
            .WithMessage("End time is required.")
            .GreaterThan(x => x.Start)
            .WithMessage("End time must be after start time.");

        RuleFor(x => x)
            .Must(x => (x.End - x.Start).TotalMinutes >= 15)
            .WithMessage("Appointment duration must be at least 15 minutes.")
            .Must(x => (x.End - x.Start).TotalHours <= 4)
            .WithMessage("Appointment duration cannot exceed 4 hours.");
    }
}
