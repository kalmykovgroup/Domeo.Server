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
        public const string Brands = "brands";

        // Relative paths for methods - Categories
        public const string Tree = "tree";
        public const string CategoryAttributes = "{id}/attributes";

        // Relative paths for methods - Suppliers
        public const string SupplierById = "{id}";

        // Relative paths for methods - Items (Materials)
        public const string ItemById = "{id}";
        public const string ItemOffers = "{id}/offers";
        public const string Suggest = "suggest";
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

        /// <summary>
        /// Alternative route for material categories: /api/material-categories -> /categories
        /// </summary>
        public const string MaterialCategories = "/api/material-categories";
    }

    /// <summary>
    /// External Supplier API endpoints (MockSupplier service)
    /// </summary>
    public static class SupplierApi
    {
        public const string Categories = "/api/categories";
        public const string Suppliers = "/api/suppliers";
        public const string Materials = "/api/materials";
        public const string Offers = "/api/offers";
        public const string Brands = "/api/brands";

        public static string CategoriesTree(bool activeOnly = true) =>
            $"{Categories}/tree?activeOnly={activeOnly.ToString().ToLower()}";

        public static string CategoriesList(bool activeOnly = true) =>
            $"{Categories}?activeOnly={activeOnly.ToString().ToLower()}";

        public static string CategoryAttributes(string categoryId) =>
            $"{Categories}/{categoryId}/attributes";

        public static string SuppliersList(bool activeOnly = true) =>
            $"{Suppliers}?activeOnly={activeOnly.ToString().ToLower()}";

        public static string SupplierById(string id) =>
            $"{Suppliers}/{id}";

        public static string BrandsList(bool activeOnly = true) =>
            $"{Brands}?activeOnly={activeOnly.ToString().ToLower()}";

        public static string MaterialsList(
            bool activeOnly = true,
            string? categoryId = null,
            string? brandId = null,
            string? supplierId = null,
            Dictionary<string, string>? attributes = null)
        {
            var url = $"{Materials}?activeOnly={activeOnly.ToString().ToLower()}";
            if (!string.IsNullOrEmpty(categoryId))
                url += $"&categoryId={categoryId}";
            if (!string.IsNullOrEmpty(brandId))
                url += $"&brandId={brandId}";
            if (!string.IsNullOrEmpty(supplierId))
                url += $"&supplierId={supplierId}";
            if (attributes is { Count: > 0 })
            {
                foreach (var (name, value) in attributes)
                    url += $"&attr_{name}={Uri.EscapeDataString(value)}";
            }
            return url;
        }

        public static string MaterialById(string id) =>
            $"{Materials}/{id}";

        public static string OffersByMaterial(string materialId) =>
            $"{Offers}?materialId={materialId}";

        public static string Suggest(string query, int limit = 10) =>
            $"{Materials}/suggest?query={Uri.EscapeDataString(query)}&limit={limit}";
    }
}
