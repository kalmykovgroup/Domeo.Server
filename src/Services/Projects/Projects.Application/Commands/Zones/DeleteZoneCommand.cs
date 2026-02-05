using MediatR;

namespace Projects.Application.Commands.Zones;

public sealed record DeleteZoneCommand(Guid EdgeId, Guid ZoneId) : IRequest;
