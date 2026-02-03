using System.Security.Claims;

namespace Domeo.ApiGateway.Auth;

/// <summary>
/// Middleware that extracts claims from authenticated user and adds them to request headers
/// for downstream microservices
/// </summary>
public class ClaimsForwardingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClaimsForwardingMiddleware> _logger;

    // Header names for forwarding claims
    public const string UserIdHeader = "X-User-Id";
    public const string UserEmailHeader = "X-User-Email";
    public const string UserNameHeader = "X-User-Name";
    public const string UserRoleHeader = "X-User-Role";

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
            var email = context.User.FindFirst(ClaimTypes.Email)?.Value
                     ?? context.User.FindFirst("email")?.Value;
            var name = context.User.FindFirst(ClaimTypes.Name)?.Value
                    ?? context.User.FindFirst("name")?.Value;
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value
                    ?? context.User.FindFirst("role")?.Value;

            // Add claims to headers for downstream services
            if (!string.IsNullOrEmpty(userId))
                context.Request.Headers[UserIdHeader] = userId;

            if (!string.IsNullOrEmpty(email))
                context.Request.Headers[UserEmailHeader] = email;

            if (!string.IsNullOrEmpty(name))
                context.Request.Headers[UserNameHeader] = name;

            if (!string.IsNullOrEmpty(role))
                context.Request.Headers[UserRoleHeader] = role;

            _logger.LogDebug("Forwarding claims for user {UserId} with role {Role}", userId, role);
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
