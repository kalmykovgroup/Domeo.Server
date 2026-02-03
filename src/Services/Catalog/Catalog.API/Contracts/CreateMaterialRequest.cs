namespace Catalog.API.Contracts;

public sealed record CreateMaterialRequest(
    Guid CategoryId,
    string Name,
    string? Description,
    string Unit,
    string? Color,
    string? TextureUrl);
