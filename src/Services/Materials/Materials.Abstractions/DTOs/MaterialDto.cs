namespace Materials.Abstractions.DTOs;

public sealed record MaterialDto(
    string Id,
    string CategoryId,
    string? BrandId,
    string? BrandName,
    string Name,
    string? Description,
    string Unit,
    string? Color,
    string? TextureUrl,
    bool IsActive);
