namespace Materials.Contracts.DTOs;

public sealed record SupplierDto(
    string Id,
    string Company,
    string? ContactName,
    string? Email,
    string? Phone,
    string? Address,
    string? Website,
    double Rating,
    bool IsActive);
