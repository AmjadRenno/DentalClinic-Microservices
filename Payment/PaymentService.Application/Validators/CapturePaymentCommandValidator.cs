using PaymentService.Application.Commands;
using FluentValidation;

namespace PaymentService.Application.Validators;

public class CapturePaymentCommandValidator : AbstractValidator<CapturePaymentCommand>
{
    public CapturePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required.");
    }
}
