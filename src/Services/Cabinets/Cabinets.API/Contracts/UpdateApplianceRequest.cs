namespace Cabinets.API.Contracts;

public sealed record UpdateApplianceRequest(
    double PositionX,
    double PositionY,
    double PositionZ,
    Guid? CabinetId);
