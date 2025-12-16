using Dapr;
using Dapr.Client;
using DentalClinic.SharedKernel.DomainEvents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application;
using PaymentService.Domain.Entities;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly PaymentApplicationService _service;

    public PaymentsController(PaymentApplicationService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [Topic("pubsub", "appointments.confirmed")]
    [HttpPost("/appointments/confirmed")]
    public async Task<IActionResult> OnAppointmentConfirmed([FromBody] AppointmentConfirmedEvent evt)
    {
        await _service.HandleAppointmentConfirmed(evt);
        return Ok();
    }

    // NOTE: Authorization can be enabled here if needed in future versions
    [HttpPost]
    public async Task<IActionResult> Create(Guid paymentId, Guid appointmentId, decimal total)
    {
        await _service.HandleCreate(paymentId, appointmentId, total);
        return Ok();
    }

    [HttpPut("{id}/authorize")]
    public async Task<IActionResult> Authorize(Guid id)
    {
        await _service.HandleAuthorize(id);
        return Ok();
    }

    [HttpPut("{id}/capture")]
    public async Task<IActionResult> Capture(Guid id)
    {
        await _service.HandleCapture(id);
        return Ok();
    }

    [HttpPut("{id}/refund")]
    public async Task<IActionResult> Refund(Guid id)
    {
        await _service.HandleRefund(id);
        return Ok();
    }
}
