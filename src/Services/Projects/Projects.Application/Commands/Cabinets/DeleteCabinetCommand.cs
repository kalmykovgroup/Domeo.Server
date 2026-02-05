using MediatR;

namespace Projects.Application.Commands.Cabinets;

public sealed record DeleteCabinetCommand(Guid Id) : IRequest;
