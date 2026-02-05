namespace Projects.Contracts.DTOs.Zones;

public sealed record CreateZoneRequest(
    string Type,
    double StartX,
    double EndX,
    string? Name = null);
