using MediatR;
using Projects.Contracts.DTOs.CabinetHardwareOverrides;

namespace Projects.Application.Commands.CabinetHardwareOverrides;

public sealed record GetOverrideByIdQuery(Guid Id) : IRequest<CabinetHardwareOverrideDto>;
