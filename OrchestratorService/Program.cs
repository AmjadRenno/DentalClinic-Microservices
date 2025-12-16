using Dapr.Client;
using Dapr.Workflow;

var builder = WebApplication.CreateBuilder(args);

//  Shared Aspire settings (Telemetry, Health, Service Discovery)
builder.AddServiceDefaults();

// Add Dapr client + Workflow
builder.Services.AddDaprClient();
builder.Services.AddDaprWorkflow(opts =>
{
    opts.RegisterWorkflow<AppointmentWorkflow>();

    // Register the three activities
    opts.RegisterActivity<ValidateAppointmentActivity>();
    opts.RegisterActivity<ReserveSlotActivity>();
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

// Endpoint to start the workflow
app.MapPost("appointment/start", async (AppointmentInput input, DaprWorkflowClient wf, ILogger<Program> logger) =>
{
    logger.LogInformation("Starting appointment workflow for {PatientName} on {Date}", input.PatientName, input.Date);
    var instanceId = Guid.NewGuid().ToString("N");

    await wf.ScheduleNewWorkflowAsync(nameof(AppointmentWorkflow), instanceId, input);

    return Results.Ok(new { instanceId });
});

// Endpoint to track status + return Output (AppointmentResult)
app.MapGet("appointment/status/{id}", async (string id, DaprWorkflowClient wf) =>
{
    var state = await wf.GetWorkflowStateAsync(id);

    if (state is null || !state.Exists)
    {
        return Results.NotFound(new { message = "Workflow instance not found." });
    }

    // Return the same state that you saw in Swagger (exists, isWorkflowCompleted, ...)
    return Results.Ok(new
    {
        state.Exists,
        state.IsWorkflowRunning,
        state.IsWorkflowCompleted,
        state.CreatedAt,
        state.LastUpdatedAt,
        state.RuntimeStatus
    });
});


app.MapDefaultEndpoints();
app.Run();


// ----------------- WORKFLOW -----------------

public class AppointmentWorkflow : Workflow<AppointmentInput, AppointmentResult>
{
    public override async Task<AppointmentResult> RunAsync(WorkflowContext ctx, AppointmentInput input)
    {
        // 1️⃣ Validate the appointment (date + daily limits)
        var validation = await ctx.CallActivityAsync<ValidationResult>(nameof(ValidateAppointmentActivity), input);
        if (!validation.Success)
        {
            return new AppointmentResult(false, $"Validation failed: {validation.Message}");
        }

        // 2️⃣ Reserve (lock) the Timeslot in the state store
        var reservation = await ctx.CallActivityAsync<ReservationResult>(nameof(ReserveSlotActivity), input);
        if (!reservation.Success)
        {
            return new AppointmentResult(false, $"Reservation failed: {reservation.Message}");
        }

        // 3️⃣ Send notification (Log for patient and dentist)
        var notification = await ctx.CallActivityAsync<NotificationResult>(nameof(SendNotificationActivity), input);
        if (!notification.Success)
        {
            return new AppointmentResult(false, $"Notification failed: {notification.Message}");
        }

        return new AppointmentResult(true, "Appointment validated, reserved, and notifications sent.");
    }
}


// ------------- Activity 1: ValidateAppointment -------------

public class ValidateAppointmentActivity : WorkflowActivity<AppointmentInput, ValidationResult>
{
    private readonly ILogger<ValidateAppointmentActivity> _logger;

    public ValidateAppointmentActivity(ILogger<ValidateAppointmentActivity> logger)
    {
        _logger = logger;
    }

    public override Task<ValidationResult> RunAsync(WorkflowActivityContext ctx, AppointmentInput input)
    {
        _logger.LogInformation("Validating appointment for {PatientName} on {Date}", input.PatientName, input.Date);

        // 1) Date must be today or in the future
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        if (input.Date < today)
        {
            return Task.FromResult(new ValidationResult(false, "Date is in the past."));
        }

        // 2) No more than N bookings in the same day for the same patient (simple logic)
        const int MaxPerDay = 3;
        if (input.NumberOfAppointmentsToday >= MaxPerDay)
        {
            return Task.FromResult(new ValidationResult(false,
                $"Patient already has {input.NumberOfAppointmentsToday} appointments today."));
        }

        return Task.FromResult(new ValidationResult(true, "Validation succeeded."));
    }
}


// ------------- Activity 2: ReserveSlot (state store) -------------

public class ReserveSlotActivity : WorkflowActivity<AppointmentInput, ReservationResult>
{
    private readonly DaprClient _dapr;
    private readonly ILogger<ReserveSlotActivity> _logger;

    public ReserveSlotActivity(DaprClient dapr, ILogger<ReserveSlotActivity> logger)
    {
        _dapr = dapr;
        _logger = logger;
    }

    public override async Task<ReservationResult> RunAsync(WorkflowActivityContext ctx, AppointmentInput input)
    {
        // Simplified slot reservation: one slot per patient per day
        var slotKey = $"slot-{input.PatientName}-{input.Date:yyyyMMdd}";

        _logger.LogInformation("Trying to reserve slot {SlotKey} in Dapr state store...", slotKey);

        // Try to read the value from state store
        var existing = await _dapr.GetStateAsync<string>("statestore", slotKey);

        if (!string.IsNullOrEmpty(existing))
        {
            _logger.LogWarning("Slot {SlotKey} is already reserved.", slotKey);
            return new ReservationResult(false, "This time slot is already reserved for this patient.");
        }

        // Reserve the slot
        await _dapr.SaveStateAsync("statestore", slotKey, "reserved");

        _logger.LogInformation("Slot {SlotKey} reserved successfully.", slotKey);
        return new ReservationResult(true, "Slot reserved.");
    }
}


// ------------- Activity 3: SendNotification -------------

public class SendNotificationActivity : WorkflowActivity<AppointmentInput, NotificationResult>
{
    private readonly ILogger<SendNotificationActivity> _logger;

    public SendNotificationActivity(ILogger<SendNotificationActivity> logger)
    {
        _logger = logger;
    }

    public override async Task<NotificationResult> RunAsync(WorkflowActivityContext ctx, AppointmentInput input)
    {
        _logger.LogInformation("Sending notifications for {PatientName} on {Date}...", input.PatientName, input.Date);

        // Here just a Log – in the future can connect to Email/SMS or Umbraco
        await Task.Delay(500);

        _logger.LogInformation("Notification to patient {PatientName}: Your appointment on {Date} is confirmed.",
            input.PatientName, input.Date);
        _logger.LogInformation("Notification to dentist: New confirmed appointment for patient {PatientName} on {Date}.",
            input.PatientName, input.Date);

        return new NotificationResult(true, "Notifications sent.");
    }
}


// --------- DTOs / Records used in the Workflow ---------

public record AppointmentInput(
    string PatientName,
    DateOnly Date,
    int NumberOfAppointmentsToday,
    decimal Amount // Available for future use
);

public record ValidationResult(bool Success, string Message);
public record ReservationResult(bool Success, string Message);
public record NotificationResult(bool Success, string Message);
public record AppointmentResult(bool Success, string Message);
