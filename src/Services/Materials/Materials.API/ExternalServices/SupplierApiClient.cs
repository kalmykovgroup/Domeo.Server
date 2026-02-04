using System.Net.Http.Json;

namespace Materials.API.ExternalServices;

public sealed class SupplierApiClient : ISupplierApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SupplierApiClient> _logger;

    public SupplierApiClient(HttpClient httpClient, ILogger<SupplierApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<ExternalCategoryTreeNode>> GetCategoriesTreeAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/categories/tree?activeOnly={activeOnly.ToString().ToLower()}";
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<List<ExternalCategoryTreeNode>>>(url, ct);

            if (response?.Success == true && response.Data != null)
                return response.Data;

            _logger.LogWarning("Failed to get categories tree from supplier API: {Message}", response?.Message);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories tree from supplier API");
            throw;
        }
    }

    public async Task<List<ExternalCategory>> GetCategoriesAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/categories?activeOnly={activeOnly.ToString().ToLower()}";
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<List<ExternalCategory>>>(url, ct);

            if (response?.Success == true && response.Data != null)
                return response.Data;

            _logger.LogWarning("Failed to get categories from supplier API: {Message}", response?.Message);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories from supplier API");
            throw;
        }
    }

    public async Task<List<ExternalSupplier>> GetSuppliersAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/suppliers?activeOnly={activeOnly.ToString().ToLower()}";
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<List<ExternalSupplier>>>(url, ct);

            if (response?.Success == true && response.Data != null)
                return response.Data;

            _logger.LogWarning("Failed to get suppliers from supplier API: {Message}", response?.Message);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suppliers from supplier API");
            throw;
        }
    }

    public async Task<ExternalSupplier?> GetSupplierAsync(string id, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/suppliers/{id}";
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<ExternalSupplier>>(url, ct);

            if (response?.Success == true)
                return response.Data;

            _logger.LogWarning("Failed to get supplier {Id} from supplier API: {Message}", id, response?.Message);
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting supplier {Id} from supplier API", id);
            throw;
        }
    }

    public async Task<List<ExternalMaterial>> GetMaterialsAsync(string? categoryId = null, bool activeOnly = true, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/materials?activeOnly={activeOnly.ToString().ToLower()}";
            if (!string.IsNullOrEmpty(categoryId))
                url += $"&categoryId={categoryId}";

            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<List<ExternalMaterial>>>(url, ct);

            if (response?.Success == true && response.Data != null)
                return response.Data;

            _logger.LogWarning("Failed to get materials from supplier API: {Message}", response?.Message);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting materials from supplier API");
            throw;
        }
    }

    public async Task<ExternalMaterial?> GetMaterialAsync(string id, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/materials/{id}";
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<ExternalMaterial>>(url, ct);

            if (response?.Success == true)
                return response.Data;

            _logger.LogWarning("Failed to get material {Id} from supplier API: {Message}", id, response?.Message);
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting material {Id} from supplier API", id);
            throw;
        }
    }

    public async Task<ExternalOffersData?> GetOffersAsync(string materialId, CancellationToken ct = default)
    {
        try
        {
            var url = $"/api/offers?materialId={materialId}";
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<ExternalOffersData>>(url, ct);

            if (response?.Success == true)
                return response.Data;

            _logger.LogWarning("Failed to get offers for material {MaterialId} from supplier API: {Message}", materialId, response?.Message);
            return null;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting offers for material {MaterialId} from supplier API", materialId);
            throw;
        }
    }
}
