namespace Materials.Abstractions.ExternalServices;

public interface ISupplierApiClient
{
    Task<List<ExternalCategoryTreeNode>> GetCategoriesTreeAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<List<ExternalCategory>> GetCategoriesAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<List<ExternalSupplier>> GetSuppliersAsync(bool activeOnly = true, CancellationToken ct = default);
    Task<ExternalSupplier?> GetSupplierAsync(string id, CancellationToken ct = default);
    Task<List<ExternalMaterial>> GetMaterialsAsync(string? categoryId = null, bool activeOnly = true, CancellationToken ct = default);
    Task<ExternalMaterial?> GetMaterialAsync(string id, CancellationToken ct = default);
    Task<ExternalOffersData?> GetOffersAsync(string materialId, CancellationToken ct = default);
}
