using PaymentService.Domain.Entities;
using PaymentService.Domain.ValueObjects;
using PaymentService.Domain.Tests.TestHelpers;
using FluentAssertions;
using Xunit;

namespace PaymentService.Domain.Tests.EntityTests;

public class PaymentTests
{
    [Fact]
    public void Constructor_ShouldCreatePayment_WithPendingStatus()
    {
        // Arrange
        var id = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var money = new Money(100.00m, "USD");

        // Act
        var payment = new Payment(id, appointmentId, money);

        // Assert
        payment.Id.Should().Be(id);
        payment.AppointmentId.Should().Be(appointmentId);
        payment.Total.Should().Be(money);
        payment.Status.Should().Be(PaymentStatus.Pending);
    }

    [Fact]
    public void Payments_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var id = Guid.NewGuid();
        var payment1 = PaymentBuilder.CreateWithId(id);
        var payment2 = PaymentBuilder.CreateWithId(id);

        // Act & Assert
        payment1.Should().Be(payment2);
        (payment1 == payment2).Should().BeTrue();
    }

    [Fact]
    public void Payments_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var payment1 = PaymentBuilder.Create();
        var payment2 = PaymentBuilder.Create();

        // Act & Assert
        payment1.Should().NotBe(payment2);
        (payment1 != payment2).Should().BeTrue();
    }

    [Fact]
    public void Authorize_WhenStatusIsPending_ShouldChangeStatusToAuthorized()
    {
        // Arrange
        var payment = PaymentBuilder.Create();

        // Act
        payment.Authorize();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Authorized);
    }

    [Fact]
    public void Authorize_WhenStatusIsNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = PaymentBuilder.CreateAuthorized();

        // Act
        var act = () => payment.Authorize();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only pending payments can be authorized.");
    }

    [Theory]
    [InlineData(PaymentStatus.Authorized)]
    [InlineData(PaymentStatus.Captured)]
    [InlineData(PaymentStatus.Failed)]
    [InlineData(PaymentStatus.Refunded)]
    public void Authorize_WithInvalidStatus_ShouldThrowInvalidOperationException(PaymentStatus status)
    {
        // Arrange
        var payment = CreatePaymentWithStatus(status);

        // Act
        var act = () => payment.Authorize();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Capture_WhenStatusIsAuthorized_ShouldChangeStatusToCaptured()
    {
        // Arrange
        var payment = PaymentBuilder.CreateAuthorized();

        // Act
        payment.Capture();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Captured);
    }

    [Fact]
    public void Capture_WhenStatusIsPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = PaymentBuilder.Create();

        // Act
        var act = () => payment.Capture();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only authorized payments can be captured.");
    }

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Captured)]
    [InlineData(PaymentStatus.Failed)]
    [InlineData(PaymentStatus.Refunded)]
    public void Capture_WithInvalidStatus_ShouldThrowInvalidOperationException(PaymentStatus status)
    {
        // Arrange
        var payment = CreatePaymentWithStatus(status);

        // Act
        var act = () => payment.Capture();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Authorized)]
    [InlineData(PaymentStatus.Captured)]
    public void Fail_WithAnyStatus_ShouldChangeStatusToFailed(PaymentStatus initialStatus)
    {
        // Arrange
        var payment = CreatePaymentWithStatus(initialStatus);

        // Act
        payment.Fail();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void Refund_WhenStatusIsCaptured_ShouldChangeStatusToRefunded()
    {
        // Arrange
        var payment = PaymentBuilder.CreateCaptured();

        // Act
        payment.Refund();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void Refund_WhenStatusIsPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = PaymentBuilder.Create();

        // Act
        var act = () => payment.Refund();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only captured payments can be refunded.");
    }

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Authorized)]
    [InlineData(PaymentStatus.Failed)]
    [InlineData(PaymentStatus.Refunded)]
    public void Refund_WithInvalidStatus_ShouldThrowInvalidOperationException(PaymentStatus status)
    {
        // Arrange
        var payment = CreatePaymentWithStatus(status);

        // Act
        var act = () => payment.Refund();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void PaymentLifecycle_SuccessfulFlow_ShouldWorkCorrectly()
    {
        // Arrange
        var payment = PaymentBuilder.Create();

        // Act & Assert - Step by step lifecycle
        payment.Status.Should().Be(PaymentStatus.Pending);
        
        payment.Authorize();
        payment.Status.Should().Be(PaymentStatus.Authorized);
        
        payment.Capture();
        payment.Status.Should().Be(PaymentStatus.Captured);
    }

    [Fact]
    public void PaymentLifecycle_RefundFlow_ShouldWorkCorrectly()
    {
        // Arrange
        var payment = PaymentBuilder.Create();

        // Act
        payment.Authorize();
        payment.Capture();
        payment.Refund();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void PaymentLifecycle_FailedFlow_ShouldWorkCorrectly()
    {
        // Arrange
        var payment = PaymentBuilder.Create();

        // Act
        payment.Fail();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Theory]
    [InlineData(50.00)]
    [InlineData(100.50)]
    [InlineData(999.99)]
    [InlineData(0.01)]
    public void Payment_WithVariousAmounts_ShouldStoreCorrectly(decimal amount)
    {
        // Arrange & Act
        var payment = PaymentBuilder.Create(amount);

        // Assert
        payment.Total.Amount.Should().Be(amount);
    }

    private static Payment CreatePaymentWithStatus(PaymentStatus status)
    {
        return status switch
        {
            PaymentStatus.Pending => PaymentBuilder.Create(),
            PaymentStatus.Authorized => PaymentBuilder.CreateAuthorized(),
            PaymentStatus.Captured => PaymentBuilder.CreateCaptured(),
            PaymentStatus.Failed => PaymentBuilder.CreateFailed(),
            PaymentStatus.Refunded => PaymentBuilder.CreateRefunded(),
            _ => throw new ArgumentException($"Unknown status: {status}")
        };
    }
}
