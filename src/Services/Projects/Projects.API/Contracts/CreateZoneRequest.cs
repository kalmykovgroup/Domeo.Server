namespace Projects.API.Contracts;

public sealed record CreateZoneRequest(
    string Type,
    double StartX,
    double EndX,
    string? Name = null);
