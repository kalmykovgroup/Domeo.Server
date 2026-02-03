namespace Projects.API.Contracts;

public sealed record UpdateZoneRequest(
    string? Name,
    double StartX,
    double EndX);
