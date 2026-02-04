namespace Domeo.Shared.Routes.Routers;

/// <summary>
/// Route definitions for Materials.API service.
/// Internal paths: /categories/*, /suppliers/*, /items/*
/// </summary>
public static class MaterialsRoutes
{
    public const string ServiceName = "materials";

    // Internal paths - Categories
    public const string CategoriesTree = "/categories/tree";

    // Internal paths - Suppliers
    public const string Suppliers = "/suppliers";
    public const string SupplierById = "/suppliers/{id}";

    // Internal paths - Items (Materials)
    public const string Items = "/items";
    public const string ItemById = "/items/{id}";
    public const string ItemOffers = "/items/{id}/offers";

    // Gateway paths (external API)
    public static class Gateway
    {
        /// <summary>
        /// Main prefix - all Materials.API routes are available under /api/materials/*
        /// </summary>
        public const string Prefix = "/api/materials";

        /// <summary>
        /// Alternative route for suppliers: /api/suppliers -> /suppliers
        /// </summary>
        public const string Suppliers = "/api/suppliers";
    }
}
