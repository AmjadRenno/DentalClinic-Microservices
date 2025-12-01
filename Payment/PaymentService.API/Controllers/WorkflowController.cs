using Microsoft.AspNetCore.Mvc;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("/")]
public class WorkflowController : ControllerBase
{
    private readonly ILogger<WorkflowController> _logger;

    public WorkflowController(ILogger<WorkflowController> logger)
    {
        _logger = logger;
    }

    [HttpPost("charge")]
    public IActionResult Charge([FromBody] ChargeRequest request)
    {
        _logger.LogInformation("Processing payment for {PatientName}, amount: {Amount}", request.PatientName, request.Amount);

        if (request.Amount <= 0)
            return Ok(new PaymentResult(false, "Invalid payment amount."));

        // منطق تجريبي
        return Ok(new PaymentResult(true, $"Payment of {request.Amount} DKK completed."));
    }
}

public record ChargeRequest(string PatientName, decimal Amount);
public record PaymentResult(bool Success, string Message);
