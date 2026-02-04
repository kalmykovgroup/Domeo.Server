using Domeo.Shared.Kernel.Application.Abstractions;

namespace Clients.Abstractions.Commands;

public sealed record DeleteClientCommand(Guid Id) : ICommand;
