using System.Net;
using System.Text.Json;
using Domeo.Shared.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using StackExchange.Redis;

namespace Domeo.Shared.Infrastructure.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        var (statusCode, response) = exception switch
        {
            NpgsqlException => (
                HttpStatusCode.ServiceUnavailable,
                ApiResponse.Fail("Database temporarily unavailable")
            ),
            DbUpdateException { InnerException: NpgsqlException } => (
                HttpStatusCode.ServiceUnavailable,
                ApiResponse.Fail("Database temporarily unavailable")
            ),
            RedisConnectionException => (
                HttpStatusCode.ServiceUnavailable,
                ApiResponse.Fail("Cache service temporarily unavailable")
            ),
            RedisTimeoutException => (
                HttpStatusCode.ServiceUnavailable,
                ApiResponse.Fail("Cache service temporarily unavailable")
            ),
            ArgumentException argEx => (
                HttpStatusCode.BadRequest,
                ApiResponse.Fail(argEx.Message)
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                ApiResponse.Fail("Unauthorized access")
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                ApiResponse.Fail("Resource not found")
            ),
            InvalidOperationException opEx => (
                HttpStatusCode.BadRequest,
                ApiResponse.Fail(opEx.Message)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                ApiResponse.Fail("An unexpected error occurred")
            )
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(response, JsonOptions);
        await context.Response.WriteAsync(json);
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
