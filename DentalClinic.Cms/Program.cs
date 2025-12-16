using DentalClinic.Cms.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IAppointmentApiClient, AppointmentApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7168");
});

builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7168"); // Gateway URL
});

// Simple in-memory dentist service (no external API)
builder.Services.AddSingleton<IDentistApiClient, InMemoryDentistApiClient>();

builder.Services.AddHttpClient("Gateway", client =>
{
    client.BaseAddress = new Uri("https://localhost:7168"); // Adjust the value according to your API Gateway URL
});

builder.Services.AddHttpClient("PaymentService", client =>
{
    client.BaseAddress = new Uri("http://localhost:5012");
});




builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

// HSTS only outside Development
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Redirect to HTTPS
app.UseHttpsRedirection();

// Session before Umbraco
app.UseSession();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
