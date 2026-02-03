using Domeo.Shared.Kernel.Domain.Results;
using MediatR;

namespace Domeo.Shared.Kernel.Application.Abstractions;

/// <summary>
/// Marker interface for queries (read operations)
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;

/// <summary>
/// Handler for queries
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
