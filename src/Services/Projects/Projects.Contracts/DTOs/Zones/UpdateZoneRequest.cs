namespace Projects.Contracts.DTOs.Zones;

public sealed record UpdateZoneRequest(
    string? Name,
    double StartX,
    double EndX);
