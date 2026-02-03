namespace Domeo.Shared.Contracts.DTOs;

public sealed record SupplierDto(
    Guid Id,
    string Company,
    string? ContactFirstName,
    string? ContactLastName,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);
