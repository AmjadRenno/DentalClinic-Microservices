namespace PaymentService.Application.Commands;

public sealed record AuthorizePaymentCommand(Guid PaymentId);
