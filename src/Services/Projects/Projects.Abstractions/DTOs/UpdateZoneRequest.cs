namespace Projects.Abstractions.DTOs;

public sealed record UpdateZoneRequest(
    string? Name,
    double StartX,
    double EndX);
