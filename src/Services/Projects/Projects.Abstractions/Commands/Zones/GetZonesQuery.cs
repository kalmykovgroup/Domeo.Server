using MediatR;
using Projects.Abstractions.DTOs;

namespace Projects.Abstractions.Commands.Zones;

public sealed record GetZonesQuery(Guid EdgeId) : IRequest<List<ZoneDto>>;
