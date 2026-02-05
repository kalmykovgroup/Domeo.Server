using MediatR;
using Projects.Contracts.DTOs.CabinetHardwareOverrides;

namespace Projects.Application.Commands.CabinetHardwareOverrides;

public sealed record GetOverridesByCabinetQuery(Guid CabinetId) : IRequest<List<CabinetHardwareOverrideDto>>;
