var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); // Aspire telemetry, health, service discovery

var app = builder.Build();

// 🦷 "حجز الموعد": يقبل الحجز فقط إذا لم يتجاوز عدد المرضى اليومي 10 (كمثال بسيط)
app.MapPost("/reserve", (AppointmentRequest input, ILogger<Program> logger) =>
{
    var ok = input.NumberOfAppointmentsToday < 10;
    logger.LogInformation("Booking for {PatientName} on {Date}: {Result}",
        input.PatientName, input.Date, ok ? "CONFIRMED" : "FULLY BOOKED");

    return Results.Ok(new BookingResult(
        ok,
        ok ? "Appointment reserved successfully" : "Clinic is fully booked for today"
    ));
});

app.MapDefaultEndpoints(); // health checks etc.
app.Run();

// DTOs
internal record AppointmentRequest(string PatientName, DateOnly Date, int NumberOfAppointmentsToday);

internal record BookingResult(bool Success, string Message);
