using MediatR;
using Projects.Abstractions.DTOs;

namespace Projects.Abstractions.Commands.Cabinets;

public sealed record GetCabinetByIdQuery(Guid Id) : IRequest<CabinetDto>;
