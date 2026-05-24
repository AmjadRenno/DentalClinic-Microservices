namespace PaymentService.Application.Commands;

public sealed record CapturePaymentCommand(Guid PaymentId);
