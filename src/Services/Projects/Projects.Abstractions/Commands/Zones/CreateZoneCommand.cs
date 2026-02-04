using MediatR;

namespace Projects.Abstractions.Commands.Zones;

public sealed record CreateZoneCommand(
    Guid EdgeId,
    string Type,
    double StartX,
    double EndX,
    string? Name) : IRequest<Guid>;
