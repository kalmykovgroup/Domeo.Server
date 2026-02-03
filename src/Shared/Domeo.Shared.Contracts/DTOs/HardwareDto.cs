namespace Domeo.Shared.Contracts.DTOs;

public sealed record HardwareDto(
    int Id,
    string Type,
    string Name,
    string? Brand,
    string? Model,
    string? ModelUrl,
    string? Params,
    bool IsActive);
