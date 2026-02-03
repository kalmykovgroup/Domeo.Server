namespace Catalog.API.Contracts;

public sealed record UpdateSupplierRequest(
    string Company,
    string? ContactFirstName,
    string? ContactLastName,
    string? Email,
    string? Phone,
    string? Address,
    bool IsActive);
