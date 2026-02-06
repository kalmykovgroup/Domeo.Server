using MediatR;

namespace Modules.Application.Commands.Storage;

public sealed record MigrateToRemoteStorageCommand : IRequest<int>;
