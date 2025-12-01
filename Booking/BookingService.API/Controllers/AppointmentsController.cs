using BookingService.Application;
using BookingService.Application.Commands;
using BookingService.Application.Queries;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BookingService.Infrastructure.Data; // 👈 عدّل الـ namespace حسب BookingDbContext عندك
using Microsoft.EntityFrameworkCore;

namespace BookingService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly BookingApplicationService _appService;
    private readonly BookingDbContext _db;   // 👈 جديد

    public AppointmentsController(BookingApplicationService appService, BookingDbContext db)
    {
        _appService = appService;
        _db = db;
    }

    //[Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(RequestAppointmentCommand command)
    {
        await _appService.Handle(command);
        return Ok();
    }

    //[Authorize(Roles = "Admin")]
    [HttpPut("confirm")]
    public async Task<IActionResult> Confirm(ConfirmAppointmentCommand command)
    {
        await _appService.Handle(command);
        return Ok();
    }

    //[Authorize(Roles = "Admin")]
    [HttpPut("cancel")]
    public async Task<IActionResult> Cancel(CancelAppointmentCommand command)
    {
        await _appService.Handle(command);
        return Ok();
    }

    //[Authorize(Roles = "Admin")]
    [HttpPut("reschedule")]
    public async Task<IActionResult> Reschedule(RescheduleAppointmentCommand command)
    {
        await _appService.Handle(command);
        return Ok();
    }

    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        // نقرأ الـ userId الذي أرسله الـ Gateway
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

        // نحول الـ Domain entities إلى DTOs بسيطة
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

    // 🔹 Endpoint للأدمن — يعيد كل المواعيد
    [HttpGet("admin")]
    public async Task<IActionResult> GetAllForAdmin()
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
