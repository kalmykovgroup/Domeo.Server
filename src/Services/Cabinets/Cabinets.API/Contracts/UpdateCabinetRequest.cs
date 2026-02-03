namespace Cabinets.API.Contracts;

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
    int? ModuleTypeId,
    string? FacadeType,
    decimal? CalculatedPrice);
