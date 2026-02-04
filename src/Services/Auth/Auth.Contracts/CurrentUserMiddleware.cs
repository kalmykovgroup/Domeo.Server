using System.Security.Claims;
using Domeo.Shared.Infrastructure.Audit;
using Microsoft.AspNetCore.Http;

namespace Auth.Contracts;

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
            var role = context.User.FindFirst(ClaimTypes.Role)?.Value
                ?? context.User.FindFirst("role")?.Value
                ?? string.Empty;

            Guid? parsedUserId = Guid.TryParse(userId, out var uid) ? uid : null;

            currentUserAccessor.User = new CurrentUser
            {
                Id = parsedUserId,
                Role = role
            };

            if (parsedUserId.HasValue)
            {
                auditContextAccessor.AuditContext = new AuditContext
                {
                    UserId = parsedUserId.Value,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    CorrelationId = context.TraceIdentifier
                };
            }
        }

        await _next(context);
    }
}
