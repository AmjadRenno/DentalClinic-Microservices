using DentalClinic.SharedKernel;
using PaymentService.Domain.ValueObjects;

namespace PaymentService.Domain.Entities;

public enum PaymentStatus
{
    Pending,
    Authorized,
    Captured,
    Failed,
    Refunded
}

public sealed class Payment : AggregateRoot
{
    public Guid AppointmentId { get; init; }
    public Money Total { get; private set; }
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;

    // ✅ Constructor المستخدم في الكود العادي
    public Payment(Guid id, Guid appointmentId, Money total)
        : base(id)
    {
        AppointmentId = appointmentId;
        Total = total;
    }

    // ✅ Constructor فارغ خاص بـ EF Core
    private Payment() : base(Guid.Empty) { }

    public void Authorize()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException("Only pending payments can be authorized.");
        Status = PaymentStatus.Authorized;
    }

    public void Capture()
    {
        if (Status != PaymentStatus.Authorized)
            throw new InvalidOperationException("Only authorized payments can be captured.");
        Status = PaymentStatus.Captured;
    }

    public void Fail() => Status = PaymentStatus.Failed;

    public void Refund()
    {
        if (Status != PaymentStatus.Captured)
            throw new InvalidOperationException("Only captured payments can be refunded.");
        Status = PaymentStatus.Refunded;
    }
}
