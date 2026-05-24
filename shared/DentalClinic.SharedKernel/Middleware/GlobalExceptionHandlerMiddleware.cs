using DentalClinic.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace DentalClinic.SharedKernel.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and returns consistent Problem Details responses (RFC 7807)
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problemDetails) = exception switch
        {
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                CreateProblemDetails(
                    context,
                    "Not Found",
                    (int)HttpStatusCode.NotFound,
                    notFoundEx.Message,
                    notFoundEx.ErrorCode,
                    new Dictionary<string, object>
                    {
                        ["entityName"] = notFoundEx.EntityName,
                        ["entityId"] = notFoundEx.EntityId?.ToString() ?? string.Empty
                    }
                )
            ),

            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                CreateValidationProblemDetails(
                    context,
                    validationEx.Message,
                    validationEx.ErrorCode,
                    validationEx.Errors
                )
            ),

            BusinessRuleException businessEx => (
                HttpStatusCode.UnprocessableEntity,
                CreateProblemDetails(
                    context,
                    "Business Rule Violation",
                    (int)HttpStatusCode.UnprocessableEntity,
                    businessEx.Message,
                    businessEx.ErrorCode
                )
            ),

            ConflictException conflictEx => (
                HttpStatusCode.Conflict,
                CreateProblemDetails(
                    context,
                    "Conflict",
                    (int)HttpStatusCode.Conflict,
                    conflictEx.Message,
                    conflictEx.ErrorCode
                )
            ),

            InvalidOperationException invalidOpEx => (
                HttpStatusCode.BadRequest,
                CreateProblemDetails(
                    context,
                    "Invalid Operation",
                    (int)HttpStatusCode.BadRequest,
                    invalidOpEx.Message,
                    "INVALID_OPERATION"
                )
            ),

            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                CreateProblemDetails(
                    context,
                    "Bad Request",
                    (int)HttpStatusCode.BadRequest,
                    argEx.Message,
                    "INVALID_ARGUMENT"
                )
            ),

            _ => (
                HttpStatusCode.InternalServerError,
                CreateProblemDetails(
                    context,
                    "Internal Server Error",
                    (int)HttpStatusCode.InternalServerError,
                    "An unexpected error occurred while processing your request.",
                    "INTERNAL_ERROR"
                )
            )
        };

        // Log the exception
        LogException(exception, statusCode);

        // Set response
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        string title,
        int status,
        string detail,
        string errorCode,
        IDictionary<string, object>? extensions = null)
    {
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Status = status,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{status}"
        };

        problemDetails.Extensions["errorCode"] = errorCode;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        if (extensions != null)
        {
            foreach (var extension in extensions)
            {
                problemDetails.Extensions[extension.Key] = extension.Value;
            }
        }

        return problemDetails;
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext context,
        string detail,
        string errorCode,
        IDictionary<string, string[]> errors)
    {
        var problemDetails = new ValidationProblemDetails(errors)
        {
            Title = "Validation Failed",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{(int)HttpStatusCode.BadRequest}"
        };

        problemDetails.Extensions["errorCode"] = errorCode;
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        return problemDetails;
    }

    private void LogException(Exception exception, HttpStatusCode statusCode)
    {
        var logLevel = statusCode switch
        {
            HttpStatusCode.InternalServerError => LogLevel.Error,
            HttpStatusCode.BadRequest => LogLevel.Warning,
            HttpStatusCode.NotFound => LogLevel.Information,
            HttpStatusCode.Conflict => LogLevel.Warning,
            HttpStatusCode.UnprocessableEntity => LogLevel.Warning,
            _ => LogLevel.Error
        };

        _logger.Log(
            logLevel,
            exception,
            "Exception occurred: {ExceptionType} - {Message}",
            exception.GetType().Name,
            exception.Message
        );
    }
}
