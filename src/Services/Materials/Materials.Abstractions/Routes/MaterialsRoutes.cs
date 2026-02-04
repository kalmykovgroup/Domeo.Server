namespace Materials.Abstractions.Routes;

/// <summary>
/// Route definitions for Materials.API service.
/// </summary>
public static class MaterialsRoutes
{
    public const string ServiceName = "materials";

    /// <summary>
    /// Paths for controllers (relative, used in [Route] and [HttpGet])
    /// </summary>
    public static class Controller
    {
        // Base paths for controllers
        public const string Categories = "categories";
        public const string Suppliers = "suppliers";
        public const string Items = "items";

        // Relative paths for methods - Categories
        public const string Tree = "tree";

        // Relative paths for methods - Suppliers
        public const string SupplierById = "{id}";

        // Relative paths for methods - Items (Materials)
        public const string ItemById = "{id}";
        public const string ItemOffers = "{id}/offers";
    }

    /// <summary>
    /// Paths for API Gateway (full paths for YARP)
    /// </summary>
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
