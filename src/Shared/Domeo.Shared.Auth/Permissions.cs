namespace Domeo.Shared.Auth;

/// <summary>
/// Permission format: resource:action
/// Actions: read, write, delete, manage (full access)
/// </summary>
public static class Permissions
{
    // Users
    public const string UsersRead = "users:read";
    public const string UsersWrite = "users:write";
    public const string UsersDelete = "users:delete";
    public const string UsersManage = "users:manage";

    // Clients
    public const string ClientsRead = "clients:read";
    public const string ClientsWrite = "clients:write";
    public const string ClientsDelete = "clients:delete";
    public const string ClientsManage = "clients:manage";

    // Projects
    public const string ProjectsRead = "projects:read";
    public const string ProjectsWrite = "projects:write";
    public const string ProjectsDelete = "projects:delete";
    public const string ProjectsManage = "projects:manage";

    // Cabinets (project elements)
    public const string CabinetsRead = "cabinets:read";
    public const string CabinetsWrite = "cabinets:write";
    public const string CabinetsDelete = "cabinets:delete";

    // Catalog
    public const string CatalogRead = "catalog:read";
    public const string CatalogWrite = "catalog:write";
    public const string CatalogDelete = "catalog:delete";
    public const string CatalogManage = "catalog:manage";
    public const string CatalogPurchasePrice = "catalog:purchase-price";

    // Facades
    public const string FacadesRead = "facades:read";
    public const string FacadesSync = "facades:sync";

    // RBAC
    public const string RolesRead = "roles:read";
    public const string RolesWrite = "roles:write";
    public const string RolesAssign = "roles:assign";
    public const string PermissionsAssign = "permissions:assign";

    // Suppliers
    public const string SuppliersRead = "suppliers:read";
    public const string SuppliersWrite = "suppliers:write";
    public const string SuppliersDelete = "suppliers:delete";
    public const string SuppliersManage = "suppliers:manage";

    // Audit
    public const string AuditRead = "audit:read";

    // System
    public const string SystemManage = "system:manage";
}
