using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;   // 👈 أضف هذا

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// --- JWT Authentication Settings ---
var jwtKey = builder.Configuration["Jwt:Key"];
var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];

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

// CORS
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

// ✅ 1) نفعّل الـ JWT في الـ Gateway
app.UseAuthentication();

// ✅ 2) Middleware يقرأ الـ userId من الـ JWT ويحقنه كـ X-UserId
app.Use(async (context, next) =>
{
    Console.WriteLine($"🔍 Gateway: Path={context.Request.Path}, IsAuthenticated={context.User?.Identity?.IsAuthenticated}");
    
    // إذا المستخدم مصادق عليه (عبر JWT Authentication Middleware)
    if (context.User?.Identity?.IsAuthenticated == true)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            Console.WriteLine($"✅ Gateway: Found userId from authenticated user: {userId}");
            context.Request.Headers["X-UserId"] = userId;
        }
    }
    else
    {
        // إذا لم يكن authenticated، نحاول قراءة التوكن يدوياً من الـ Authorization header
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        Console.WriteLine($"🔑 Gateway: Authorization header = {(authHeader != null ? "Present" : "Missing")}");
        
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                
                // نستخرج الـ userId من الـ claims - نحاول كل الأنواع الممكنة
                var userId = jwtToken.Claims.FirstOrDefault(c => 
                    c.Type == ClaimTypes.NameIdentifier || 
                    c.Type == "sub" || 
                    c.Type == "userId" ||
                    c.Type == "nameid")?.Value;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine($"✅ Gateway: Extracted userId from JWT: {userId}");
                    context.Request.Headers["X-UserId"] = userId;
                }
                else
                {
                    Console.WriteLine($"⚠️ Gateway: Could not find userId in JWT claims. Available claims:");
                    foreach (var claim in jwtToken.Claims)
                    {
                        Console.WriteLine($"   - {claim.Type} = {claim.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Gateway: Failed to parse JWT: {ex.Message}");
            }
        }
    }

    await next();
});

app.UseAuthorization();

// ✅ 3) YARP Gateway
app.MapReverseProxy();

app.MapDefaultEndpoints();

app.Run();
