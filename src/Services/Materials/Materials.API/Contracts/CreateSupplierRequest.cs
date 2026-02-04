namespace Materials.API.Contracts;

public sealed record CreateSupplierRequest(
    string Company,
    string? ContactFirstName,
    string? ContactLastName,
    string? Email,
    string? Phone,
    string? Address);
