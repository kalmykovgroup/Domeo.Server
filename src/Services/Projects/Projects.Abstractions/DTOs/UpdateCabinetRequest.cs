namespace Projects.Abstractions.DTOs;

public sealed record UpdateCabinetRequest(
    double PositionX,
    double PositionY,
    double Rotation,
    double Width,
    double Height,
    double Depth,
    string? Name,
    Guid? EdgeId,
    Guid? ZoneId,
    Guid? AssemblyId,
    string? FacadeType,
    decimal? CalculatedPrice);
