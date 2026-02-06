using MediatR;

namespace Modules.Application.Commands.Storage;

public sealed record TestStorageConnectionCommand(Guid Id) : IRequest<bool>;
