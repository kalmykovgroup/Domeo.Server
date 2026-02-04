using MediatR;
using Projects.Abstractions.DTOs;

namespace Projects.Abstractions.Commands.CabinetHardwareOverrides;

public sealed record GetOverrideByIdQuery(Guid Id) : IRequest<CabinetHardwareOverrideDto>;
