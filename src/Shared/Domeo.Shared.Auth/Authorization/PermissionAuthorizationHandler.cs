using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Domeo.Shared.Auth.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _serviceProvider;

    public PermissionAuthorizationHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userId = GetUserId(context.User);

        // 1. Check JWT permissions claim first (fast path)
        var permissionsClaim = context.User.FindFirst("permissions")?.Value;
        if (!string.IsNullOrEmpty(permissionsClaim))
        {
            var jwtPermissions = permissionsClaim.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (jwtPermissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
                return;
            }
        }

        // 2. Try IPermissionChecker if registered (database/cache lookup)
        if (userId != Guid.Empty)
        {
            var permissionChecker = _serviceProvider.GetService<IPermissionChecker>();
            if (permissionChecker is not null)
            {
                try
                {
                    var hasPermission = await permissionChecker.HasPermissionAsync(userId, requirement.Permission);
                    if (hasPermission)
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
                catch
                {
                    // Fall through to static role check
                }
            }
        }

        // 3. Fallback to static role-based permissions
        var roleClaim = context.User.FindFirst("role")?.Value;
        if (!string.IsNullOrEmpty(roleClaim) && RolePermissions.HasPermission(roleClaim, requirement.Permission))
        {
            context.Succeed(requirement);
            return;
        }

        // 4. Check multiple roles if present
        var roles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
        foreach (var role in roles)
        {
            if (RolePermissions.HasPermission(role, requirement.Permission))
            {
                context.Succeed(requirement);
                return;
            }
        }
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? user.FindFirst("sub")?.Value
                          ?? user.FindFirst("userId")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
