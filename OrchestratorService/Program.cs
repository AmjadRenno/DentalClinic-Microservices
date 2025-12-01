using Dapr.Client;
using Dapr.Workflow;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();  // Aspire telemetry, health, service discovery

// إضافة Dapr client + Workflow
builder.Services.AddDaprClient();
builder.Services.AddDaprWorkflow(opts =>
{
    opts.RegisterWorkflow<AppointmentWorkflow>();
    opts.RegisterActivity<ReserveAppointmentActivity>();
    opts.RegisterActivity<ProcessPaymentActivity>();
    opts.RegisterActivity<SendNotificationActivity>();

});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// API لبدء workflow والتحقق من الحالة
app.MapPost("appointment/start", async (AppointmentInput input, DaprWorkflowClient wf, ILogger<Program> logger) =>
{
    logger.LogInformation("Starting appointment workflow for {PatientName} on {Date}", input.PatientName, input.Date);
    var instanceId = Guid.NewGuid().ToString("N");
    await wf.ScheduleNewWorkflowAsync(nameof(AppointmentWorkflow), instanceId, input);
    return Results.Ok(new { instanceId });
});

app.MapGet("appointment/status/{id}", async (string id, DaprWorkflowClient wf) =>
{
    var state = await wf.GetWorkflowStateAsync(id);
    return Results.Ok(state);
});

app.MapDefaultEndpoints();
app.Run();


// ---------- Workflow + Activities ----------

public class AppointmentWorkflow : Workflow<AppointmentInput, AppointmentResult>
{
    public override async Task<AppointmentResult> RunAsync(WorkflowContext ctx, AppointmentInput input)
    {
        // 1️⃣ الحجز
        var booking = await ctx.CallActivityAsync<BookingResult>(nameof(ReserveAppointmentActivity), input);
        if (!booking.Success)
            return new AppointmentResult(false, $"Booking failed: {booking.Message}");

        // 2️⃣ الدفع
        var payment = await ctx.CallActivityAsync<PaymentResult>(nameof(ProcessPaymentActivity), input);
        if (!payment.Success)
            return new AppointmentResult(false, $"Payment failed: {payment.Message}");

        // 3️⃣ الإشعار
        var notify = await ctx.CallActivityAsync<NotificationResult>(nameof(SendNotificationActivity), input);
        if (!notify.Success)
            return new AppointmentResult(false, $"Notification failed: {notify.Message}");

        return new AppointmentResult(true, "Appointment booked, payment processed, and notification sent!");
    }
}



// 🦷 النشاط الأول: ReserveAppointmentActivity
public class ReserveAppointmentActivity : WorkflowActivity<AppointmentInput, BookingResult>
{
    private readonly DaprClient _dapr;
    private readonly ILogger<ReserveAppointmentActivity> _logger;

    public ReserveAppointmentActivity(DaprClient dapr, ILogger<ReserveAppointmentActivity> logger)
    {
        _dapr = dapr;
        _logger = logger;
    }

    public override async Task<BookingResult> RunAsync(WorkflowActivityContext ctx, AppointmentInput input)
    {
        _logger.LogInformation("Calling BookingService from Orchestrator...");

        var http = DaprClient.CreateInvokeHttpClient("bookingservice");
        var resp = await http.PostAsJsonAsync("/reserve", new
        {
            input.PatientName,
            input.Date,
            input.NumberOfAppointmentsToday
        });

        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<BookingResult>() ?? new(false, "No response");
    }
}


// 💳 النشاط الثاني: ProcessPaymentActivity
public class ProcessPaymentActivity : WorkflowActivity<AppointmentInput, PaymentResult>
{
    private readonly DaprClient _dapr;
    private readonly ILogger<ProcessPaymentActivity> _logger;

    public ProcessPaymentActivity(DaprClient dapr, ILogger<ProcessPaymentActivity> logger)
    {
        _dapr = dapr;
        _logger = logger;
    }

    public override async Task<PaymentResult> RunAsync(WorkflowActivityContext ctx, AppointmentInput input)
    {
        _logger.LogInformation("Calling PaymentService from Orchestrator...");

        var http = DaprClient.CreateInvokeHttpClient("paymentservice");
        var resp = await http.PostAsJsonAsync("/charge", new
        {
            input.PatientName,
            input.Amount
        });

        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<PaymentResult>() ?? new(false, "No response");
    }
}

// 📩 النشاط الثالث: SendNotificationActivity
public class SendNotificationActivity : WorkflowActivity<AppointmentInput, NotificationResult>
{
    private readonly ILogger<SendNotificationActivity> _logger;

    public SendNotificationActivity(ILogger<SendNotificationActivity> logger)
    {
        _logger = logger;
    }

    public override async Task<NotificationResult> RunAsync(WorkflowActivityContext ctx, AppointmentInput input)
    {
        _logger.LogInformation("Sending notification to {PatientName}...", input.PatientName);

        // منطق تجريبي (يمكن لاحقًا ربطه بخدمة خارجية)
        await Task.Delay(1000); // محاكاة تأخير إرسال إشعار
        _logger.LogInformation("Notification sent successfully to {PatientName}", input.PatientName);

        return new NotificationResult(true, $"Notification sent to {input.PatientName}");
    }
}

public record NotificationResult(bool Success, string Message);

// Shared DTOs
public record AppointmentInput(string PatientName, DateOnly Date, int NumberOfAppointmentsToday, decimal Amount);
public record BookingResult(bool Success, string Message);
public record PaymentResult(bool Success, string Message);
public record AppointmentResult(bool Success, string Message);
