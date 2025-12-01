using Microsoft.EntityFrameworkCore;
using PaymentService.Application;
using PaymentService.Application.Interfaces;
using PaymentService.Infrastructure.Data;
using PaymentService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// ---------- SERVICES ----------
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlite("Data Source=PaymentService.db"));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<PaymentApplicationService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// JWT Settings
var jwtKey = "this_is_my_super_secret_key_12345";
var issuer = "DentalClinicAuth";
var audience = "DentalClinicServices";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();


// ---------- APP ----------
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();


// تأكيد إنشاء قاعدة البيانات
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.EnsureCreated();
}

// Middleware
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Payment Service API v1");
    });
}

app.UseCloudEvents();  // ✅ دعم CloudEvents
app.MapSubscribeHandler();  // ✅ يفعّل endpoint /dapr/subscribe
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
