namespace Catalog.API.Contracts;

public sealed record UpdateMaterialRequest(
    string Name,
    string? Description,
    string? Color,
    string? TextureUrl,
    bool IsActive);
