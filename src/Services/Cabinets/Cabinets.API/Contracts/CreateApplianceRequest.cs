namespace Cabinets.API.Contracts;

public sealed record CreateApplianceRequest(
    Guid ProjectId,
    string Type,
    double PositionX,
    double PositionY,
    double PositionZ,
    double Width,
    double Height,
    double Depth,
    Guid? CabinetId);
