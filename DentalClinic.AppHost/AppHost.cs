using Aspire.Hosting;
using CommunityToolkit.Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

//  Booking Service (DDD Architecture)
var booking = builder
    .AddProject("bookingservice", @"..\Booking\BookingService.API\BookingService.API.csproj")
    .WithDaprSidecar(new DaprSidecarOptions { AppId = "bookingservice" });

// Payment Service (DDD Architecture)
var payment = builder
    .AddProject("paymentservice", @"..\Payment\PaymentService.API\PaymentService.API.csproj")
    .WithDaprSidecar(new DaprSidecarOptions { AppId = "paymentservice" });

//  Auth Service (JWT + BCrypt)
var auth = builder
    .AddProject("authservice", @"..\AuthService.API\AuthService.API.csproj");

// Orchestrator (Dapr Workflow)
var orchestrator = builder
    .AddProject("orchestratorservice", @"..\OrchestratorService\OrchestratorService.csproj")
    .WithReference(booking)
    .WithReference(payment)
    .WaitFor(booking)
    .WaitFor(payment)
    .WithDaprSidecar(new DaprSidecarOptions { AppId = "orchestratorservice" });

//  Gateway (YARP + JWT)
var gateway = builder
    .AddProject("gatewayservice", @"..\GatewayService\GatewayService.csproj")
    .WithReference(orchestrator)
    .WithReference(auth)
    .WaitFor(auth)
    .WaitFor(orchestrator);

//  Frontend (Umbraco CMS)
var cms = builder
    .AddProject("umbraco", @"..\DentalClinic.Cms\DentalClinic.Cms.csproj")
    .WithReference(gateway)
    .WaitFor(gateway);

builder.Build().Run();
