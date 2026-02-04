using MediatR;

namespace Projects.Abstractions.Commands.CabinetHardwareOverrides;

public sealed record DeleteOverrideCommand(Guid Id) : IRequest;
