using Microsoft.EntityFrameworkCore;
using PaymentService.Application;
using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Repositories;
using DentalClinic.SharedKernel.Middleware;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// ---------- SERVICES ----------
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlite("Data Source=PaymentService.db"));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<PaymentApplicationService>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<PaymentApplicationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();


// ---------- APP ----------
var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.EnsureCreated();
}

// Global Exception Handler (must be early in the pipeline)
app.UseGlobalExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Payment Service API v1");
    });
}


// Dapr
app.UseCloudEvents();
app.MapSubscribeHandler();

app.MapControllers();
app.Run();

// Make the Program class accessible to integration tests
public partial class Program { }
