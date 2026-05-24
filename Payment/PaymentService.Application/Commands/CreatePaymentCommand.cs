namespace PaymentService.Application.Commands;

public sealed record CreatePaymentCommand(
    Guid PaymentId,
    Guid AppointmentId,
    decimal Amount,
    string Currency = "DKK");
