using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace DentalClinic.SharedKernel.Middleware;

/// <summary>
/// Security headers middleware to protect against common web vulnerabilities
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // X-Content-Type-Options: Prevents MIME type sniffing
        context.Response.Headers["X-Content-Type-Options"] = "nosniff";

        // X-Frame-Options: Prevents clickjacking attacks
        context.Response.Headers["X-Frame-Options"] = "DENY";

        // X-XSS-Protection: Legacy XSS filter (for older browsers)
        context.Response.Headers["X-XSS-Protection"] = "1; mode=block";

        // Strict-Transport-Security: Enforces HTTPS (only add in production)
        if (!context.Request.Host.Host.Contains("localhost"))
        {
            context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        // Content-Security-Policy: Restricts resource loading
        context.Response.Headers["Content-Security-Policy"] = 
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data: https:; " +
            "font-src 'self' data:; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none';";

        // Referrer-Policy: Controls referrer information
        context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Permissions-Policy: Controls browser features
        context.Response.Headers["Permissions-Policy"] = 
            "geolocation=(), " +
            "microphone=(), " +
            "camera=(), " +
            "payment=(), " +
            "usb=(), " +
            "magnetometer=(), " +
            "gyroscope=(), " +
            "accelerometer=()";

        await _next(context);
    }
}

/// <summary>
/// Extension methods for security headers middleware
/// </summary>
public static class SecurityHeadersMiddlewareExtensions
{
    /// <summary>
    /// Adds security headers to HTTP responses
    /// </summary>
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
