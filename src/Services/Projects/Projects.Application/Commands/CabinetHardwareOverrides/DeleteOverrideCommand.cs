using MediatR;

namespace Projects.Application.Commands.CabinetHardwareOverrides;

public sealed record DeleteOverrideCommand(Guid Id) : IRequest;
