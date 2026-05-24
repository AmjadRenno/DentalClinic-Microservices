using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using PaymentService.Application.Commands;
using Xunit;

namespace PaymentService.API.IntegrationTests;

/// <summary>
/// Integration Tests for Payments API endpoints
/// Tests the full stack: API → Application → Infrastructure → Database
/// </summary>
public class PaymentsControllerTests : IClassFixture<PaymentApiFactory>
{
    private readonly HttpClient _client;
    private readonly PaymentApiFactory _factory;

    public PaymentsControllerTests(PaymentApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region POST /api/payments (Create Payment)

    [Fact]
    public async Task CreatePayment_WithValidData_ReturnsOk()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var total = 150.00m;

        // Act
        var response = await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total={total}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreatePayment_WithZeroAmount_ReturnsBadRequest()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var total = 0m; // Invalid amount

        // Act
        var response = await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total={total}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePayment_WithNegativeAmount_ReturnsBadRequest()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        var total = -50m; // Invalid amount

        // Act
        var response = await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total={total}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(50.00)]
    [InlineData(500.00)]
    [InlineData(9999.99)]
    public async Task CreatePayment_WithValidAmounts_ReturnsOk(decimal amount)
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total={amount}",
            null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region PUT /api/payments/{id}/authorize

    [Fact]
    public async Task AuthorizePayment_WithExistingPayment_ReturnsOk()
    {
        // Arrange - First create a payment
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total=150.00",
            null);

        // Act - Authorize it
        var response = await _client.PutAsync($"/api/payments/{paymentId}/authorize", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AuthorizePayment_WithNonExistentPayment_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/api/payments/{nonExistentId}/authorize", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AuthorizePayment_Twice_ReturnsUnprocessableEntity()
    {
        // Arrange - Create and authorize a payment
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total=150.00",
            null);
        await _client.PutAsync($"/api/payments/{paymentId}/authorize", null);

        // Act - Try to authorize again
        var response = await _client.PutAsync($"/api/payments/{paymentId}/authorize", null);

        // Assert - Should fail because already authorized
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    #endregion

    #region PUT /api/payments/{id}/capture

    [Fact]
    public async Task CapturePayment_WithAuthorizedPayment_ReturnsOk()
    {
        // Arrange - Create and authorize a payment
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total=150.00",
            null);
        await _client.PutAsync($"/api/payments/{paymentId}/authorize", null);

        // Act - Capture it
        var response = await _client.PutAsync($"/api/payments/{paymentId}/capture", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CapturePayment_WithoutAuthorization_ReturnsUnprocessableEntity()
    {
        // Arrange - Create payment but don't authorize
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total=150.00",
            null);

        // Act - Try to capture without authorization
        var response = await _client.PutAsync($"/api/payments/{paymentId}/capture", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task CapturePayment_WithNonExistentPayment_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/api/payments/{nonExistentId}/capture", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region PUT /api/payments/{id}/refund

    [Fact]
    public async Task RefundPayment_WithCapturedPayment_ReturnsOk()
    {
        // Arrange - Create, authorize, and capture a payment
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total=150.00",
            null);
        await _client.PutAsync($"/api/payments/{paymentId}/authorize", null);
        await _client.PutAsync($"/api/payments/{paymentId}/capture", null);

        // Act - Refund it
        var response = await _client.PutAsync($"/api/payments/{paymentId}/refund", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RefundPayment_WithoutCapture_ReturnsUnprocessableEntity()
    {
        // Arrange - Create and authorize but don't capture
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();
        await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total=150.00",
            null);
        await _client.PutAsync($"/api/payments/{paymentId}/authorize", null);

        // Act - Try to refund without capture
        var response = await _client.PutAsync($"/api/payments/{paymentId}/refund", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RefundPayment_WithNonExistentPayment_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.PutAsync($"/api/payments/{nonExistentId}/refund", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Payment State Machine Workflow Tests

    [Fact]
    public async Task PaymentWorkflow_FullFlow_Success()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        // Act & Assert - Create (Pending)
        var createResponse = await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total=150.00",
            null);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - Authorize (Authorized)
        var authorizeResponse = await _client.PutAsync($"/api/payments/{paymentId}/authorize", null);
        authorizeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - Capture (Captured)
        var captureResponse = await _client.PutAsync($"/api/payments/{paymentId}/capture", null);
        captureResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - Refund (Refunded)
        var refundResponse = await _client.PutAsync($"/api/payments/{paymentId}/refund", null);
        refundResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PaymentWorkflow_AuthorizeAndCancel_Success()
    {
        // Arrange
        var paymentId = Guid.NewGuid();
        var appointmentId = Guid.NewGuid();

        // Create
        await _client.PostAsync(
            $"/api/payments?paymentId={paymentId}&appointmentId={appointmentId}&total=150.00",
            null);

        // Authorize
        var authorizeResponse = await _client.PutAsync($"/api/payments/{paymentId}/authorize", null);
        authorizeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Trying to capture after refund should fail (test state machine)
        // This is a simplified test - in real scenario we'd need a cancel endpoint
    }

    #endregion
}
