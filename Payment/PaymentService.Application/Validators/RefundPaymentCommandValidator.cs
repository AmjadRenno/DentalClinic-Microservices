using PaymentService.Application.Commands;
using FluentValidation;

namespace PaymentService.Application.Validators;

public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
{
    public RefundPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .NotEmpty()
            .WithMessage("Payment ID is required.");
    }
}
