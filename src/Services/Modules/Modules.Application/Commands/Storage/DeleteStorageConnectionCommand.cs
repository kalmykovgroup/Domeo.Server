using MediatR;

namespace Modules.Application.Commands.Storage;

public sealed record DeleteStorageConnectionCommand(Guid Id) : IRequest;
