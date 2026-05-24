using PaymentService.Application.Commands;
using FluentValidation;

namespace PaymentService.Application.Validators;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required.");

        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("Appointment ID is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.")
            .LessThanOrEqualTo(100000)
            .WithMessage("Amount cannot exceed 100,000.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required.")
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code.")
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be uppercase letters only.");
    }
}
