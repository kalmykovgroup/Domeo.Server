using Projects.Contracts.DTOs.CabinetParts;

namespace Projects.Contracts.DTOs.Cabinets;

public sealed record CreateCabinetRequest(
    Guid RoomId,
    string PlacementType,
    double PositionX,
    double PositionY,
    Dictionary<string, double>? ParameterOverrides = null,
    string? Name = null,
    Guid? EdgeId = null,
    Guid? ZoneId = null,
    Guid? AssemblyId = null,
    string? FacadeType = null,
    double Rotation = 0,
    List<CreateCabinetPartRequest>? Parts = null);
