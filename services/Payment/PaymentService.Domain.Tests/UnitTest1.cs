using PaymentService.Domain.Entities;
using PaymentService.Domain.ValueObjects;
using Xunit;

namespace PaymentService.Domain.Tests;

public class PaymentTests
{
    [Fact]
    public void Payments_with_same_id_should_be_equal()
    {
        var id = Guid.NewGuid();
        var p1 = new Payment(id, Guid.NewGuid(), 200m);
        var p2 = new Payment(id, Guid.NewGuid(), 200m);
        Assert.True(p1 == p2);
    }

    [Fact]
    public void Money_should_reject_negative_amount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Money(-50));
    }

    [Fact]
    public void Payment_should_follow_authorize_then_capture_flow()
    {
        var payment = new Payment(Guid.NewGuid(), Guid.NewGuid(), 300m);
        payment.Authorize();
        payment.Capture();

        Assert.Equal(PaymentStatus.Captured, payment.Status);
    }
}
