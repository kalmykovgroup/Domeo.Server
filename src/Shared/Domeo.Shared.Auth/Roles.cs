namespace Domeo.Shared.Auth;

public static class Roles
{
    public const string Sales = "sales";
    public const string Designer = "designer";
    public const string CatalogAdmin = "catalogAdmin";
    public const string SystemAdmin = "systemAdmin";

    public static readonly string[] All = [Sales, Designer, CatalogAdmin, SystemAdmin];
}
