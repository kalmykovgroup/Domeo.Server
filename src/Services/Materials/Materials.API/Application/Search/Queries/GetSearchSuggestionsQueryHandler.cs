using System.Text.Json;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.ExternalServices;
using Materials.Abstractions.Queries.Search;
using MediatR;

namespace Materials.API.Application.Search.Queries;

public sealed class GetSearchSuggestionsQueryHandler : IRequestHandler<GetSearchSuggestionsQuery, List<SearchSuggestionDto>>
{
    private readonly ISupplierApiClient _supplierApiClient;
    private readonly ILogger<GetSearchSuggestionsQueryHandler> _logger;

    public GetSearchSuggestionsQueryHandler(
        ISupplierApiClient supplierApiClient,
        ILogger<GetSearchSuggestionsQueryHandler> logger)
    {
        _supplierApiClient = supplierApiClient;
        _logger = logger;
    }

    public async Task<List<SearchSuggestionDto>> Handle(
        GetSearchSuggestionsQuery request,
        CancellationToken cancellationToken)
    {
        var externalSuggestions = await _supplierApiClient.GetSearchSuggestionsAsync(
            request.Query, request.Limit, cancellationToken);

        return externalSuggestions.Select(MapToDto).ToList();
    }

    private static SearchSuggestionDto MapToDto(ExternalSearchSuggestion s)
    {
        return new SearchSuggestionDto(
            Type: s.Type,
            Text: s.Text,
            Score: s.Score,
            MaterialId: GetString(s.Metadata, "materialId"),
            CategoryId: GetString(s.Metadata, "categoryId"),
            BrandName: GetString(s.Metadata, "brandName"),
            TextureUrl: GetString(s.Metadata, "textureUrl"),
            BrandId: GetString(s.Metadata, "brandId"),
            LogoUrl: GetString(s.Metadata, "logoUrl"),
            ParentId: GetString(s.Metadata, "parentId"),
            Level: GetInt(s.Metadata, "level"),
            CategoryPath: GetString(s.Metadata, "categoryPath"),
            AttributeName: GetString(s.Metadata, "attributeName"),
            AttributeValue: GetString(s.Metadata, "attributeValue")
        );
    }

    private static string? GetString(Dictionary<string, JsonElement> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var element))
            return null;

        if (element.ValueKind == JsonValueKind.Null)
            return null;

        return element.GetString();
    }

    private static int? GetInt(Dictionary<string, JsonElement> metadata, string key)
    {
        if (!metadata.TryGetValue(key, out var element))
            return null;

        if (element.ValueKind == JsonValueKind.Null)
            return null;

        if (element.ValueKind == JsonValueKind.Number)
            return element.GetInt32();

        return null;
    }
}
