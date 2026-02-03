namespace Domeo.Shared.Contracts.DTOs;

public sealed record ZoneDto(
    Guid Id,
    Guid EdgeId,
    string? Name,
    string Type,
    double StartX,
    double EndX);
