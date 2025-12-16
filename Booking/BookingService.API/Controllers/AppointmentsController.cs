using BookingService.Application;
using BookingService.Application.Commands;
using BookingService.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using BookingService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly BookingApplicationService _appService;
    private readonly BookingDbContext _db;

    public AppointmentsController(BookingApplicationService appService, BookingDbContext db)
    {
        _appService = appService;
        _db = db;
    }

    // JWT validation is handled in the API Gateway
    [HttpPost]
    public async Task<IActionResult> Create(RequestAppointmentCommand command)
    {
        await _appService.Handle(command);
        return Ok();
    }

    // JWT validation is handled in the API Gateway
    [HttpPut("confirm")]
    public async Task<IActionResult> Confirm(ConfirmAppointmentCommand command)
    {
        await _appService.Handle(command);
        return Ok();
    }

    // JWT validation is handled in the API Gateway
    [HttpPut("cancel")]
    public async Task<IActionResult> Cancel(CancelAppointmentCommand command)
    {
        await _appService.Handle(command);
        return Ok();
    }

    // JWT validation is handled in the API Gateway
    [HttpPut("reschedule")]
    public async Task<IActionResult> Reschedule(RescheduleAppointmentCommand command)
    {
        await _appService.Handle(command);
        return Ok();
    }

    // JWT validation is handled in the API Gateway
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        // Read the userId sent by the Gateway
        var userId = Request.Headers["X-UserId"].FirstOrDefault();

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("No X-UserId header");
        }

        if (!Guid.TryParse(userId, out var patientGuid))
        {
            return Unauthorized("Invalid X-UserId format");
        }

        var query = HttpContext.RequestServices.GetRequiredService<GetAppointmentsByPatientQuery>();
        var appointments = await query.Handle(patientGuid);

        // Convert Domain entities to simple DTOs
        var result = appointments.Select(a => new
        {
            id = a.Id,
            status = a.Status.ToString(),
            dentistId = a.DentistId.Value.ToString(),
            slot = new
            {
                date = a.Slot.Start.ToString("yyyy-MM-dd"),
                time = a.Slot.Start.ToString("HH:mm")
            }
        }).ToList();

        return Ok(result);
    }

    // JWT validation is handled in the API Gateway
    [HttpGet("dentist")]
    public async Task<IActionResult> GetAllForDentist()
    {
        var appointments = await _db.Appointments.ToListAsync();

        var result = appointments.Select(a => new
        {
            id = a.Id,
            status = a.Status.ToString(),
            dentistId = a.DentistId.Value.ToString(),
            slot = new
            {
                date = a.Slot.Start.ToString("yyyy-MM-dd"),
                time = a.Slot.Start.ToString("HH:mm")
            }
        }).ToList();

        return Ok(result);
    }
}
