using Dapr;
using Microsoft.AspNetCore.Mvc;
using DentalClinic.SharedKernel.DomainEvents;

namespace NotificationService.API.Controllers;

[ApiController]
[Route("/")]
public class NotificationsController : ControllerBase
{
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(ILogger<NotificationsController> logger)
    {
        _logger = logger;
    }

    [HttpPost("appointments/confirmed/notify")]
    [Topic("pubsub", "appointments.confirmed")]
    public IActionResult OnAppointmentConfirmed([FromBody] AppointmentConfirmedEvent evt)
    {
        _logger.LogInformation(
            "📩 [NotificationService] Appointment confirmed for PatientId={PatientId}, AppointmentId={AppointmentId}. Sending notification...",
            evt.PatientId, evt.AppointmentId);

        return Ok(new { message = "Notification processed by NotificationService" });
    }
}
