namespace Projects.Contracts.DTOs.Cabinets;

public sealed record UpdateCabinetRequest(
    double PositionX,
    double PositionY,
    double Rotation,
    Dictionary<string, double>? ParameterOverrides = null,
    string? Name = null,
    Guid? EdgeId = null,
    Guid? ZoneId = null,
    Guid? AssemblyId = null,
    string? FacadeType = null,
    decimal? CalculatedPrice = null);
