using BookingService.Application.Commands;
using FluentValidation;

namespace BookingService.Application.Validators;

public class RescheduleAppointmentCommandValidator : AbstractValidator<RescheduleAppointmentCommand>
{
    public RescheduleAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("Appointment ID is required.");

        RuleFor(x => x.NewStart)
            .NotEmpty()
            .WithMessage("New start time is required.")
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("New appointment time must be in the future.");

        RuleFor(x => x.NewEnd)
            .NotEmpty()
            .WithMessage("New end time is required.")
            .GreaterThan(x => x.NewStart)
            .WithMessage("New end time must be after start time.");

        RuleFor(x => x)
            .Must(x => (x.NewEnd - x.NewStart).TotalMinutes >= 15)
            .WithMessage("Appointment duration must be at least 15 minutes.")
            .Must(x => (x.NewEnd - x.NewStart).TotalHours <= 4)
            .WithMessage("Appointment duration cannot exceed 4 hours.");
    }
}
