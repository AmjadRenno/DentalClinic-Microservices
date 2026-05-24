using System.Net;
using System.Net.Http.Json;
using BookingService.Application.Commands;
using FluentAssertions;
using Xunit;

namespace BookingService.API.IntegrationTests;

/// <summary>
/// Integration Tests for Appointments API endpoints
/// Tests the full stack: API → Application → Infrastructure → Database
/// </summary>
public class AppointmentsControllerTests : IClassFixture<BookingApiFactory>
{
    private readonly HttpClient _client;
    private readonly BookingApiFactory _factory;

    public AppointmentsControllerTests(BookingApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    #region POST /api/appointments (Request Appointment)

    [Fact]
    public async Task RequestAppointment_WithValidData_ReturnsOk()
    {
        // Arrange
        var appointmentId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var dentistId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddDays(1);
        var end = start.AddHours(1);
        
        var command = new RequestAppointmentCommand(appointmentId, patientId, dentistId, start, end);

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RequestAppointment_WithPastDate_ReturnsBadRequest()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(-1);
        var command = new RequestAppointmentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 
            start, start.AddHours(1));

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RequestAppointment_WithEmptyPatientId_ReturnsBadRequest()
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(1);
        var command = new RequestAppointmentCommand(
            Guid.NewGuid(), Guid.Empty, Guid.NewGuid(), 
            start, start.AddHours(1));

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(200)]
    public async Task RequestAppointment_WithInvalidDurationMinutes_ReturnsBadRequest(int minutes)
    {
        // Arrange
        var start = DateTime.UtcNow.AddDays(1);
        var command = new RequestAppointmentCommand(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 
            start, start.AddMinutes(minutes));

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/appointments/confirm

    [Fact]
    public async Task ConfirmAppointment_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var command = new ConfirmAppointmentCommand(Guid.Empty);

        // Act
        var response = await _client.PutAsJsonAsync("/api/appointments/confirm", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/appointments/cancel

    [Fact]
    public async Task CancelAppointment_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var command = new CancelAppointmentCommand(Guid.Empty);

        // Act
        var response = await _client.PutAsJsonAsync("/api/appointments/cancel", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/appointments/reschedule

    [Fact]
    public async Task RescheduleAppointment_WithPastDate_ReturnsBadRequest()
    {
        // Arrange
        var newStart = DateTime.UtcNow.AddDays(-1);
        var command = new RescheduleAppointmentCommand(
            Guid.NewGuid(), newStart, newStart.AddHours(1));

        // Act
        var response = await _client.PutAsJsonAsync("/api/appointments/reschedule", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RescheduleAppointment_WithInvalidDuration_ReturnsBadRequest()
    {
        // Arrange
        var newStart = DateTime.UtcNow.AddDays(2);
        var command = new RescheduleAppointmentCommand(
            Guid.NewGuid(), newStart, newStart.AddMinutes(5));

        // Act
        var response = await _client.PutAsJsonAsync("/api/appointments/reschedule", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/appointments/mine

    [Fact]
    public async Task GetMyAppointments_WithoutUserId_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments/mine");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMyAppointments_WithValidUserId_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-UserId", userId);

        // Act
        var response = await client.GetAsync("/api/appointments/mine");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region GET /api/appointments/dentist

    [Fact]
    public async Task GetAllForDentist_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/appointments/dentist");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
