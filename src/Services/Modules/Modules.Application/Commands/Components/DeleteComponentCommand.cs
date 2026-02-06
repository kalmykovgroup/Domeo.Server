using MediatR;

namespace Modules.Application.Commands.Components;

public sealed record DeleteComponentCommand(Guid Id) : IRequest;
