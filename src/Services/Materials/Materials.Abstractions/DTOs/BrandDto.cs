namespace Materials.Abstractions.DTOs;

public sealed record BrandDto(
    string Id,
    string Name,
    string? LogoUrl,
    bool IsActive);
