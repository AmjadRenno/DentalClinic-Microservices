using PaymentService.Application.Interfaces;
using PaymentService.Domain.Entities;
using PaymentService.Domain.ValueObjects;
using DentalClinic.SharedKernel.DomainEvents;

namespace PaymentService.Application;

public class PaymentApplicationService
{
    private readonly IPaymentRepository _repository;

    public PaymentApplicationService(IPaymentRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAppointmentConfirmed(AppointmentConfirmedEvent evt)
    {
        var payment = new Payment(
            Guid.NewGuid(),
            evt.AppointmentId,
            new Money(500, "DKK") // Fixed amount for MVP
        );

        await _repository.AddAsync(payment);
    }

    public async Task HandleCreate(Guid paymentId, Guid appointmentId, decimal totalAmount)
    {
        var payment = new Payment(paymentId, appointmentId, new Money(totalAmount));
        await _repository.AddAsync(payment);
    }

    public async Task HandleAuthorize(Guid paymentId)
    {
        var payment = await _repository.GetByIdAsync(paymentId)
            ?? throw new InvalidOperationException("Payment not found.");

        payment.Authorize();
        await _repository.UpdateAsync(payment);
    }

    public async Task HandleCapture(Guid paymentId)
    {
        var payment = await _repository.GetByIdAsync(paymentId)
            ?? throw new InvalidOperationException("Payment not found.");

        payment.Capture();
        await _repository.UpdateAsync(payment);
    }

    public async Task HandleRefund(Guid paymentId)
    {
        var payment = await _repository.GetByIdAsync(paymentId)
            ?? throw new InvalidOperationException("Payment not found.");

        payment.Refund();
        await _repository.UpdateAsync(payment);
    }
}
