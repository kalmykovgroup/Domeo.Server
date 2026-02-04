namespace Projects.Abstractions.DTOs;

public sealed record CreateZoneRequest(
    string Type,
    double StartX,
    double EndX,
    string? Name = null);
