using PaymentService.Domain.Entities;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Domain.Tests.TestHelpers;

/// <summary>
/// Builder pattern for creating test Payment instances
/// </summary>
public static class PaymentBuilder
{
    public static Payment Create(decimal amount = 100.00m, string currency = "USD")
    {
        return new Payment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            new Money(amount, currency));
    }

    public static Payment CreateWithId(Guid id, decimal amount = 100.00m)
    {
        return new Payment(
            id,
            Guid.NewGuid(),
            new Money(amount, "USD"));
    }

    public static Payment CreateWithAppointmentId(Guid appointmentId, decimal amount = 100.00m)
    {
        return new Payment(
            Guid.NewGuid(),
            appointmentId,
            new Money(amount, "USD"));
    }

    public static Payment CreateWithMoney(Money money)
    {
        return new Payment(
            Guid.NewGuid(),
            Guid.NewGuid(),
            money);
    }

    public static Payment CreateAuthorized(decimal amount = 100.00m)
    {
        var payment = Create(amount);
        payment.Authorize();
        return payment;
    }

    public static Payment CreateCaptured(decimal amount = 100.00m)
    {
        var payment = Create(amount);
        payment.Authorize();
        payment.Capture();
        return payment;
    }

    public static Payment CreateFailed(decimal amount = 100.00m)
    {
        var payment = Create(amount);
        payment.Fail();
        return payment;
    }

    public static Payment CreateRefunded(decimal amount = 100.00m)
    {
        var payment = Create(amount);
        payment.Authorize();
        payment.Capture();
        payment.Refund();
        return payment;
    }

    public static Money CreateMoney(decimal amount = 100.00m, string currency = "USD")
    {
        return new Money(amount, currency);
    }
}
