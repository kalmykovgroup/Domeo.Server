using System.Security.Claims;
using Auth.Contracts.Routes;

namespace Domeo.ApiGateway.Auth;

/// <summary>
/// Middleware that extracts claims from authenticated user and adds them to request headers
/// for downstream microservices
/// </summary>
public class ClaimsForwardingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClaimsForwardingMiddleware> _logger;

    public ClaimsForwardingMiddleware(RequestDelegate next, ILogger<ClaimsForwardingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? context.User.FindFirst("sub")?.Value;
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value
                    ?? context.User.FindFirst("role")?.Value;

            if (!string.IsNullOrEmpty(userId))
                context.Request.Headers[AuthRoutes.Headers.UserId] = userId;

            if (!string.IsNullOrEmpty(role))
                context.Request.Headers[AuthRoutes.Headers.UserRole] = role;

            _logger.LogDebug("Forwarding user {UserId} with role {Role}", userId, role);
        }

        await _next(context);
    }
}

public static class ClaimsForwardingMiddlewareExtensions
{
    public static IApplicationBuilder UseClaimsForwarding(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ClaimsForwardingMiddleware>();
    }
}
