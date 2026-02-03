using System.Security.Claims;
using Domeo.Shared.Infrastructure.Audit;
using Microsoft.AspNetCore.Http;

namespace Domeo.Shared.Auth;

public sealed class CurrentUserMiddleware
{
    private readonly RequestDelegate _next;

    public CurrentUserMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICurrentUserAccessor currentUserAccessor,
        IAuditContextAccessor auditContextAccessor)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.User.FindFirst("sub")?.Value;
            var email = context.User.FindFirst(ClaimTypes.Email)?.Value
                ?? context.User.FindFirst("email")?.Value;
            var name = context.User.FindFirst(ClaimTypes.Name)?.Value
                ?? context.User.FindFirst("name")?.Value;

            // Extract role from claims
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value
                ?? context.User.FindFirst("role")?.Value
                ?? string.Empty;

            // Extract all roles from claims
            var roles = context.User.FindAll(ClaimTypes.Role)
                .Concat(context.User.FindAll("role"))
                .Select(c => c.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            Guid? parsedUserId = Guid.TryParse(userId, out var uid) ? uid : null;

            currentUserAccessor.User = new CurrentUser
            {
                Id = parsedUserId,
                Email = email ?? string.Empty,
                Name = name ?? string.Empty,
                Role = role
            };

            // Set roles using the setter
            if (currentUserAccessor is CurrentUserAccessor accessor)
            {
                accessor.Roles = roles;
                accessor.Permissions = [];
            }

            if (parsedUserId.HasValue)
            {
                auditContextAccessor.AuditContext = new AuditContext
                {
                    UserId = parsedUserId.Value,
                    UserEmail = email ?? string.Empty,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    CorrelationId = context.TraceIdentifier
                };
            }
        }

        await _next(context);
    }
}
