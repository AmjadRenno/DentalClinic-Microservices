using BookingService.Application;
using BookingService.Application.Interfaces;
using BookingService.Application.Queries;
using BookingService.Infrastructure.Data;
using BookingService.Infrastructure.Repositories;
using DentalClinic.SharedKernel.Middleware;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- SERVICES ----------
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlite("Data Source=BookingService.db"));

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<BookingApplicationService>();
builder.Services.AddScoped<GetAppointmentsByPatientQuery>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<BookingApplicationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddDaprClient();


// ---------- APP ----------
var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Database.EnsureCreated();
}

// Global Exception Handler (must be early in the pipeline)
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Booking Service API v1");
    });
}


// Dapr
app.UseCloudEvents();
app.MapSubscribeHandler();

app.MapControllers();

app.Run();

// Make the Program class accessible to integration tests
public partial class Program { }
