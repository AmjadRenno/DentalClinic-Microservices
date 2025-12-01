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

builder.Services.AddHttpClient<IUserRegistrationApiClient, UserRegistrationApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7168"); // Gateway URL
});

builder.Services.AddHttpClient<IDentistApiClient, DentistApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7168"); // Gateway URL
});

builder.Services.AddHttpClient("Gateway", client =>
{
    client.BaseAddress = new Uri("https://localhost:7168"); // عدّل القيمة حسب URL الـ API Gateway
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

// 🟢 جلسة قبل Umbraco
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
