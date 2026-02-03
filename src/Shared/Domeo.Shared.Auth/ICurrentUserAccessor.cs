namespace Domeo.Shared.Auth;

public interface ICurrentUserAccessor
{
    CurrentUser? User { get; set; }
    IReadOnlyList<string> Roles { get; }
    IReadOnlyList<string> Permissions { get; }
    bool HasRole(string role);
    bool HasPermission(string permission);
    bool HasAnyPermission(params string[] permissions);
    bool HasAllPermissions(params string[] permissions);
}
