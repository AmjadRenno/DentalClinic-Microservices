var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); // Aspire telemetry, health, service discovery

var app = builder.Build();

// 💳 الدفع ناجح إذا المبلغ أقل من 1000 كرونة مثلاً
app.MapPost("/charge", (PaymentRequest input, ILogger<Program> logger) =>
{
    var ok = input.Amount <= 1000m;
    logger.LogInformation("Charging {Amount:C} for {PatientName}: {Result}",
        input.Amount, input.PatientName, ok ? "APPROVED" : "DECLINED");

    return Results.Ok(new PaymentResult(
        ok,
        ok ? "Payment processed successfully" : "Payment declined - limit exceeded"
    ));
});

app.MapDefaultEndpoints();
app.Run();

internal record PaymentRequest(string PatientName, decimal Amount);

internal record PaymentResult(bool Success, string Message);
