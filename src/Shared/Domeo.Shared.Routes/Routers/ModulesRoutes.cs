namespace Domeo.Shared.Routes.Routers;

/// <summary>
/// Route definitions for Modules.API service.
/// Internal paths: /categories/*, /types/*, /hardware/*
/// </summary>
public static class ModulesRoutes
{
    public const string ServiceName = "modules";

    // Internal paths - Categories
    public const string Categories = "/categories";
    public const string CategoriesTree = "/categories/tree";

    // Internal paths - Types (3D modules)
    public const string Types = "/types";
    public const string TypesCount = "/types/count";
    public const string TypeById = "/types/{id:int}";

    // Internal paths - Hardware
    public const string Hardware = "/hardware";
    public const string HardwareById = "/hardware/{id:int}";

    // Gateway paths (external API)
    public static class Gateway
    {
        /// <summary>
        /// Main prefix - all Modules.API routes are available under /api/modules/*
        /// </summary>
        public const string Prefix = "/api/modules";

        /// <summary>
        /// Alternative route for module types: /api/module-types -> /types
        /// </summary>
        public const string ModuleTypes = "/api/module-types";

        /// <summary>
        /// Alternative route for module categories: /api/module-categories -> /categories
        /// </summary>
        public const string ModuleCategories = "/api/module-categories";
    }
}
