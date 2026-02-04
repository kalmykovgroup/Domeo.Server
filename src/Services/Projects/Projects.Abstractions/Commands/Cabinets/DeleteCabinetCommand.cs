using MediatR;

namespace Projects.Abstractions.Commands.Cabinets;

public sealed record DeleteCabinetCommand(Guid Id) : IRequest;
