namespace Materials.Contracts.DTOs;

public sealed record SearchSuggestionDto(
    string Type,
    string Text,
    double Score,
    string? MaterialId,
    string? CategoryId,
    string? BrandName,
    string? TextureUrl,
    string? BrandId,
    string? LogoUrl,
    string? ParentId,
    int? Level,
    string? CategoryPath,
    string? AttributeName,
    string? AttributeValue);
