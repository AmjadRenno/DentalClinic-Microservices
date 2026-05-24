namespace PaymentService.Application.Commands;

public sealed record RefundPaymentCommand(Guid PaymentId);
