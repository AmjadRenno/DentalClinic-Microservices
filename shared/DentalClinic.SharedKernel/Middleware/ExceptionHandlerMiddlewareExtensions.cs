using Microsoft.AspNetCore.Builder;

namespace DentalClinic.SharedKernel.Middleware;

/// <summary>
/// Extension methods for adding exception handling middleware
/// </summary>
public static class ExceptionHandlerMiddlewareExtensions
{
    /// <summary>
    /// Adds global exception handling middleware to the application pipeline
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
