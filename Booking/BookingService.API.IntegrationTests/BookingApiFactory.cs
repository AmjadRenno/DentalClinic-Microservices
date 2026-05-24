using BookingService.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BookingService.API.IntegrationTests;

/// <summary>
/// WebApplicationFactory for BookingService API Integration Tests
/// Uses In-Memory Database for isolated testing
/// </summary>
public class BookingApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove all DbContext-related registrations
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<BookingDbContext>) ||
                     d.ServiceType == typeof(DbContextOptions) ||
                     d.ImplementationType == typeof(BookingDbContext)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Add InMemory database for testing with unique name per test
            services.AddDbContext<BookingDbContext>(options =>
            {
                options.UseInMemoryDatabase("BookingTestDb_" + Guid.NewGuid().ToString());
            });
        });
    }
}
