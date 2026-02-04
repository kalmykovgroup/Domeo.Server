using MediatR;
using Projects.Abstractions.DTOs;

namespace Projects.Abstractions.Commands.CabinetHardwareOverrides;

public sealed record GetOverridesByCabinetQuery(Guid CabinetId) : IRequest<List<CabinetHardwareOverrideDto>>;
