namespace Domeo.Shared.Auth;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private static readonly AsyncLocal<CurrentUser?> _user = new();
    private static readonly AsyncLocal<List<string>> _roles = new();
    private static readonly AsyncLocal<List<string>> _permissions = new();

    public CurrentUser? User
    {
        get => _user.Value;
        set => _user.Value = value;
    }

    public IReadOnlyList<string> Roles
    {
        get => _roles.Value ?? [];
        set => _roles.Value = value?.ToList() ?? [];
    }

    public IReadOnlyList<string> Permissions
    {
        get => _permissions.Value ?? [];
        set => _permissions.Value = value?.ToList() ?? [];
    }

    public bool HasRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public bool HasPermission(string permission)
    {
        // Wildcard check
        if (Permissions.Contains("*"))
            return true;

        return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasAnyPermission(params string[] permissions)
    {
        if (Permissions.Contains("*"))
            return true;

        return permissions.Any(p => Permissions.Contains(p, StringComparer.OrdinalIgnoreCase));
    }

    public bool HasAllPermissions(params string[] permissions)
    {
        if (Permissions.Contains("*"))
            return true;

        return permissions.All(p => Permissions.Contains(p, StringComparer.OrdinalIgnoreCase));
    }
}
