namespace Modules.Abstractions.Routes;

/// <summary>
/// Route definitions for Modules.API service.
/// </summary>
public static class ModulesRoutes
{
    public const string ServiceName = "modules";

    /// <summary>
    /// Paths for controllers (relative, used in [Route] and [HttpGet])
    /// </summary>
    public static class Controller
    {
        // Base paths for controllers
        public const string Categories = "categories";
        public const string Types = "types";
        public const string Hardware = "hardware";

        // Relative paths for methods - Categories
        public const string Tree = "tree";

        // Relative paths for methods - Types
        public const string Count = "count";
        public const string TypeById = "{id:int}";

        // Relative paths for methods - Hardware
        public const string HardwareById = "{id:int}";
    }

    /// <summary>
    /// Paths for API Gateway (full paths for YARP)
    /// </summary>
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
