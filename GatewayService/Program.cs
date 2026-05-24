using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using AspNetCoreRateLimit;
using DentalClinic.SharedKernel.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Load rate limiting configuration
builder.Configuration.AddJsonFile("ratelimitsettings.json", optional: false, reloadOnChange: true);

// Aspire ServiceDefaults configuration
builder.AddServiceDefaults();

// --- JWT Authentication configuration in the Gateway ---
var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("Jwt:Key is missing in configuration");
var issuer = builder.Configuration["Jwt:Issuer"]
             ?? throw new InvalidOperationException("Jwt:Issuer is missing in configuration");
var audience = builder.Configuration["Jwt:Audience"]
               ?? throw new InvalidOperationException("Jwt:Audience is missing in configuration");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Same validation as in AuthService
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),

            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Authorization (so we can use [Authorize] if needed in the future)
builder.Services.AddAuthorization();

// Rate Limiting - IP based
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// CORS - Strict policies for production
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:3000", "http://localhost:5173" }; // Default for dev

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Development: Allow specific origins
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            // Production: Strict CORS
            policy.WithOrigins(allowedOrigins)
                  .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
                  .WithMethods("GET", "POST", "PUT", "DELETE")
                  .AllowCredentials()
                  .SetIsOriginAllowedToAllowWildcardSubdomains();
        }
    });
});

// YARP Reverse Proxy + Service Discovery
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Security Headers - Add early in pipeline
app.UseSecurityHeaders();

// HSTS only outside Development
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Redirect to HTTPS
app.UseHttpsRedirection();

// Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// Rate Limiting (must be before authentication)
app.UseIpRateLimiting();

// 1) Enable Authentication in the Gateway
app.UseAuthentication();

// 2) Middleware to read userId from JWT and add it to X-UserId
app.Use(async (context, next) =>
{
    // If the user is authenticated by the Authentication middleware
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            context.Request.Headers["X-UserId"] = userId;
        }
    }
    else
    {
        // If not authenticated, try to read the token directly
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();

        if (!string.IsNullOrEmpty(authHeader) &&
            authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var userId = jwtToken.Claims.FirstOrDefault(c =>
                    c.Type == ClaimTypes.NameIdentifier ||
                    c.Type == "sub" ||
                    c.Type == "userId" ||
                    c.Type == "nameid")?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    context.Request.Headers["X-UserId"] = userId;
                }
            }
            catch
            {
                // JWT parsing failed, continue without userId
            }
        }
    }

    await next();
});

// 3) Authorization + YARP
app.UseAuthorization();

app.MapReverseProxy();
app.MapDefaultEndpoints();

app.Run();

// Make Program accessible to tests
public partial class Program { }
