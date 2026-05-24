using PaymentService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PaymentService.API.IntegrationTests;

/// <summary>
/// WebApplicationFactory for PaymentService API Integration Tests
/// Uses In-Memory Database for isolated testing
/// </summary>
public class PaymentApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove all DbContext-related registrations
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<PaymentDbContext>) ||
                     d.ServiceType == typeof(DbContextOptions) ||
                     d.ImplementationType == typeof(PaymentDbContext)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database for testing with unique name per test
            services.AddDbContext<PaymentDbContext>(options =>
            {
                options.UseInMemoryDatabase("PaymentTestDb_" + Guid.NewGuid().ToString());
            });
        });
    }
}
