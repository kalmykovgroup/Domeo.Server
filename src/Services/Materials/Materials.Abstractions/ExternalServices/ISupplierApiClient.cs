namespace Materials.Abstractions.ExternalServices;

public interface ISupplierApiClient
{
    Task<List<ExternalCategoryTreeNode>> GetCategoriesTreeAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<List<ExternalCategory>> GetCategoriesAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<List<ExternalSupplier>> GetSuppliersAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<ExternalSupplier?> GetSupplierAsync(string id, CancellationToken ct = default);
    Task<List<ExternalMaterial>> GetMaterialsAsync(string? categoryId = null, bool activeOnly = true, string? brandId = null, string? supplierId = null, Dictionary<string, string>? attributes = null, CancellationToken ct = default);
    Task<ExternalMaterial?> GetMaterialAsync(string id, CancellationToken ct = default);
    Task<ExternalOffersData?> GetOffersAsync(string materialId, CancellationToken ct = default);
    Task<List<ExternalBrand>> GetBrandsAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<List<ExternalCategoryAttribute>> GetCategoryAttributesAsync(string categoryId, CancellationToken ct = default);
    Task<List<ExternalSearchSuggestion>> GetSearchSuggestionsAsync(string query, int limit = 10, CancellationToken ct = default);
}
