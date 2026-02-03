namespace Domeo.Shared.Auth;

/// <summary>
/// Maps roles to their permissions
/// </summary>
public static class RolePermissions
{
    private static readonly Dictionary<string, HashSet<string>> RoleToPermissions = new()
    {
        [Roles.Sales] =
        [
            // Can read/write/manage own clients
            Permissions.ClientsRead,
            Permissions.ClientsWrite,
            Permissions.ClientsManage,

            // Can read/write own projects
            Permissions.ProjectsRead,
            Permissions.ProjectsWrite,

            // Can work with project elements
            Permissions.CabinetsRead,
            Permissions.CabinetsWrite,

            // Can read catalog
            Permissions.CatalogRead,
            Permissions.SuppliersRead,
            Permissions.FacadesRead
        ],

        [Roles.Designer] =
        [
            // Inherit Sales permissions + delete
            Permissions.ClientsRead,
            Permissions.ClientsWrite,
            Permissions.ClientsDelete,
            Permissions.ClientsManage,
            Permissions.ProjectsRead,
            Permissions.ProjectsWrite,
            Permissions.ProjectsDelete,
            Permissions.CabinetsRead,
            Permissions.CabinetsWrite,
            Permissions.CabinetsDelete,
            Permissions.CatalogRead,
            Permissions.SuppliersRead,
            Permissions.FacadesRead
        ],

        [Roles.CatalogAdmin] =
        [
            // Read access to projects/clients
            Permissions.ClientsRead,
            Permissions.ProjectsRead,
            Permissions.CabinetsRead,

            // Full catalog management
            Permissions.CatalogRead,
            Permissions.CatalogWrite,
            Permissions.CatalogDelete,
            Permissions.CatalogManage,
            Permissions.CatalogPurchasePrice,
            Permissions.SuppliersRead,
            Permissions.SuppliersWrite,
            Permissions.SuppliersDelete,
            Permissions.SuppliersManage,
            Permissions.FacadesRead,
            Permissions.FacadesSync
        ],

        [Roles.SystemAdmin] =
        [
            // Full access to everything
            Permissions.UsersRead,
            Permissions.UsersWrite,
            Permissions.UsersDelete,
            Permissions.UsersManage,
            Permissions.ClientsRead,
            Permissions.ClientsWrite,
            Permissions.ClientsDelete,
            Permissions.ClientsManage,
            Permissions.ProjectsRead,
            Permissions.ProjectsWrite,
            Permissions.ProjectsDelete,
            Permissions.ProjectsManage,
            Permissions.CabinetsRead,
            Permissions.CabinetsWrite,
            Permissions.CabinetsDelete,
            Permissions.CatalogRead,
            Permissions.CatalogWrite,
            Permissions.CatalogDelete,
            Permissions.CatalogManage,
            Permissions.CatalogPurchasePrice,
            Permissions.SuppliersRead,
            Permissions.SuppliersWrite,
            Permissions.SuppliersDelete,
            Permissions.SuppliersManage,
            Permissions.FacadesRead,
            Permissions.FacadesSync,
            Permissions.AuditRead,
            Permissions.SystemManage,
            Permissions.RolesRead,
            Permissions.RolesWrite,
            Permissions.RolesAssign,
            Permissions.PermissionsAssign
        ]
    };

    /// <summary>
    /// Get all permissions for a role
    /// </summary>
    public static IReadOnlySet<string> GetPermissions(string role)
    {
        return RoleToPermissions.TryGetValue(role, out var permissions)
            ? permissions
            : new HashSet<string>();
    }

    /// <summary>
    /// Check if a role has a specific permission
    /// </summary>
    public static bool HasPermission(string role, string permission)
    {
        return RoleToPermissions.TryGetValue(role, out var permissions)
               && permissions.Contains(permission);
    }

    /// <summary>
    /// Check if a role has any of the specified permissions
    /// </summary>
    public static bool HasAnyPermission(string role, params string[] permissions)
    {
        if (!RoleToPermissions.TryGetValue(role, out var rolePermissions))
            return false;

        return permissions.Any(p => rolePermissions.Contains(p));
    }

    /// <summary>
    /// Check if a role has all of the specified permissions
    /// </summary>
    public static bool HasAllPermissions(string role, params string[] permissions)
    {
        if (!RoleToPermissions.TryGetValue(role, out var rolePermissions))
            return false;

        return permissions.All(p => rolePermissions.Contains(p));
    }
}
