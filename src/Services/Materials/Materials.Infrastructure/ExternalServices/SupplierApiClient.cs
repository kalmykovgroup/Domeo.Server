using System.Net.Http.Json;
using Materials.Application.ExternalServices;
using Materials.Contracts.Routes;
using Microsoft.Extensions.Logging;

namespace Materials.Infrastructure.ExternalServices;

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
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<List<ExternalCategoryTreeNode>>>(
                MaterialsRoutes.SupplierApi.CategoriesTree(activeOnly), ct);

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
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<List<ExternalCategory>>>(
                MaterialsRoutes.SupplierApi.CategoriesList(activeOnly), ct);

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
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<List<ExternalSupplier>>>(
                MaterialsRoutes.SupplierApi.SuppliersList(activeOnly), ct);

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
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<ExternalSupplier>>(
                MaterialsRoutes.SupplierApi.SupplierById(id), ct);

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

    public async Task<List<ExternalMaterial>> GetMaterialsAsync(string? categoryId = null, bool activeOnly = true, string? brandId = null, string? supplierId = null, Dictionary<string, string>? attributes = null, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<List<ExternalMaterial>>>(
                MaterialsRoutes.SupplierApi.MaterialsList(activeOnly, categoryId, brandId, supplierId, attributes), ct);

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
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<ExternalMaterial>>(
                MaterialsRoutes.SupplierApi.MaterialById(id), ct);

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
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<ExternalOffersData>>(
                MaterialsRoutes.SupplierApi.OffersByMaterial(materialId), ct);

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

    public async Task<List<ExternalBrand>> GetBrandsAsync(bool activeOnly = true, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<List<ExternalBrand>>>(
                MaterialsRoutes.SupplierApi.BrandsList(activeOnly), ct);

            if (response?.Success == true && response.Data != null)
                return response.Data;

            _logger.LogWarning("Failed to get brands from supplier API: {Message}", response?.Message);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting brands from supplier API");
            throw;
        }
    }

    public async Task<List<ExternalCategoryAttribute>> GetCategoryAttributesAsync(string categoryId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<List<ExternalCategoryAttribute>>>(
                MaterialsRoutes.SupplierApi.CategoryAttributes(categoryId), ct);

            if (response?.Success == true && response.Data != null)
                return response.Data;

            _logger.LogWarning("Failed to get category attributes for {CategoryId} from supplier API: {Message}", categoryId, response?.Message);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category attributes for {CategoryId} from supplier API", categoryId);
            throw;
        }
    }

    public async Task<List<ExternalSearchSuggestion>> GetSearchSuggestionsAsync(string query, int limit = 10, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ExternalApiResponse<List<ExternalSearchSuggestion>>>(
                MaterialsRoutes.SupplierApi.Suggest(query, limit), ct);

            if (response?.Success == true && response.Data != null)
                return response.Data;

            _logger.LogWarning("Failed to get search suggestions from supplier API: {Message}", response?.Message);
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search suggestions from supplier API");
            throw;
        }
    }
}
