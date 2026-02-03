namespace Domeo.Shared.Contracts.DTOs;

public sealed record MaterialDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string? Description,
    string Unit,
    string? Color,
    string? TextureUrl,
    bool IsActive);
