using MediatR;

namespace Projects.Application.Commands.CabinetParts;

public sealed record DeleteCabinetPartCommand(Guid Id) : IRequest;
