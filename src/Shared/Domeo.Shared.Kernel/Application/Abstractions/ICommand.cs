using Domeo.Shared.Kernel.Domain.Results;
using MediatR;

namespace Domeo.Shared.Kernel.Application.Abstractions;

/// <summary>
/// Marker interface for commands (write operations)
/// </summary>
public interface ICommand : IRequest<Result>;

/// <summary>
/// Generic command that returns a value
/// </summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

/// <summary>
/// Handler for commands without return value
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>
/// Handler for commands with return value
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
