using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

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

// CORS (wide for development, can be narrowed in production)
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// YARP Reverse Proxy + Service Discovery
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
