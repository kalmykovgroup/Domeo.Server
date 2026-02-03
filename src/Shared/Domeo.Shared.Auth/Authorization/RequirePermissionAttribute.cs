using Microsoft.AspNetCore.Authorization;

namespace Domeo.Shared.Auth.Authorization;

/// <summary>
/// Requires the user to have the specified permission
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission)
        : base($"Permission:{permission}")
    {
    }
}
