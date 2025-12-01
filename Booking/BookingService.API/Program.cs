using BookingService.Application;
using BookingService.Application.Interfaces;
using BookingService.Application.Queries;
using BookingService.Infrastructure.Data;
using BookingService.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ---------- SERVICES ----------
builder.Services.AddDbContext<BookingDbContext>(options =>
    options.UseSqlite("Data Source=BookingService.db"));

builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<BookingApplicationService>();
builder.Services.AddScoped<GetAppointmentsByPatientQuery>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddDaprClient();

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

// تأكيد إنشاء قاعدة البيانات
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookingDbContext>();
    db.Database.EnsureCreated();
}

// =========================
// 🔥 الترتيب الصحيح
// =========================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Booking Service API v1");
    });
}

// فقط في Production نستخدم HTTPS redirect
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 🟢 يجب أن يكون هنا
app.UseAuthentication();
app.UseAuthorization();

// باقي الـ Dapr
app.UseCloudEvents();
app.MapSubscribeHandler();

app.MapControllers();

app.Run();
