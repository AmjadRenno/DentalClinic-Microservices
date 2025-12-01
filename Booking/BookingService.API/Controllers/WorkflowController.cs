using Microsoft.AspNetCore.Mvc;

namespace BookingService.API.Controllers;

[ApiController]
[Route("/")]
public class WorkflowController : ControllerBase
{
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(ILogger<WorkflowController> logger)
    {
        _logger = logger;
    }

    [HttpPost("reserve")]
    public IActionResult Reserve([FromBody] ReserveRequest request)
    {
        _logger.LogInformation("Received booking request for {PatientName} on {Date}", request.PatientName, request.Date);

        // منطق تجريبي بسيط (MVP)
        if (request.NumberOfAppointmentsToday >= 10)
        {
            return Ok(new BookingResult(false, "Clinic fully booked for today."));
        }

        return Ok(new BookingResult(true, "Appointment reserved successfully."));
    }
}

public record ReserveRequest(string PatientName, DateOnly Date, int NumberOfAppointmentsToday);
public record BookingResult(bool Success, string Message);
