using Aspire.Hosting;
using CommunityToolkit.Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

// 🦷 Booking Service
var booking = builder
    .AddProject("bookingservice", @"..\Booking\BookingService.API\BookingService.API.csproj")
    .WithDaprSidecar(new DaprSidecarOptions { AppId = "bookingservice" });

// 💳 Payment Service
var payment = builder
    .AddProject("paymentservice", @"..\Payment\PaymentService.API\PaymentService.API.csproj")
    .WithDaprSidecar(new DaprSidecarOptions { AppId = "paymentservice" });

// 📩 Notification Service
var notification = builder
    .AddProject("notificationservice", @"..\NotificationService.API\NotificationService.API.csproj")
    .WithDaprSidecar(new DaprSidecarOptions { AppId = "notificationservice" })
    .WithReference(booking)
    .WaitFor(booking);

// 🔐 Auth Service  (بدون Dapr)
var auth = builder
    .AddProject("authservice", @"..\AuthService.API\AuthService.API.csproj");

// 👤 Patient Service (بدون Dapr)
var patient = builder
    .AddProject("patientservice", @"..\PatientService.API\PatientService.API.csproj");

// 🦷 Dentist Service (بدون Dapr)
var dentist = builder
    .AddProject("dentistservice", @"..\DentistService.API\DentistService.API.csproj");

// 🔁 Orchestrator (workflow)
var orchestrator = builder
    .AddProject("orchestratorservice", @"..\OrchestratorService\OrchestratorService.csproj")
    .WithReference(booking)
    .WithReference(payment)
    .WithReference(notification)
    .WaitFor(booking)
    .WaitFor(payment)
    .WithDaprSidecar(new DaprSidecarOptions { AppId = "orchestratorservice" });

// 🌐 Gateway
var gateway = builder
    .AddProject("gatewayservice", @"..\GatewayService\GatewayService.csproj")
    .WithReference(orchestrator)
    .WithReference(auth)
    .WithReference(patient)
    .WithReference(dentist)
    .WaitFor(auth)
    .WaitFor(patient)
    .WaitFor(dentist)
    .WaitFor(orchestrator);

// 💻 Frontend
 var cms = builder
    .AddProject("umbraco", @"..\DentalClinic.Cms\DentalClinic.Cms.csproj")
    .WithReference(gateway)    // اختيارياً لو تحتاج استدعاء APIs
    .WaitFor(gateway);


builder.Build().Run();
