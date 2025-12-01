using Frontend.Components;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); // Aspire telemetry, health, service discovery

// Blazor server components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient يتصل فقط عبر الـ Gateway
builder.Services.AddHttpClient<AppointmentServiceClient>(client =>
{
    client.BaseAddress = new("https+http://gatewayservice"); // Aspire service discovery
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public class AppointmentServiceClient
{
    public AppointmentServiceClient(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }
    public HttpClient HttpClient { get; }
}
