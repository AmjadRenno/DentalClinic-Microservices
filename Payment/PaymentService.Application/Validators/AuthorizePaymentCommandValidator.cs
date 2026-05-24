using PaymentService.Application.Commands;
using FluentValidation;

namespace PaymentService.Application.Validators;

public class AuthorizePaymentCommandValidator : AbstractValidator<AuthorizePaymentCommand>
{
    public AuthorizePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required.");
    }
}
