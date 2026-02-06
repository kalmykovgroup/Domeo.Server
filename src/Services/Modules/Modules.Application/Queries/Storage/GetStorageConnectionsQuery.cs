using MediatR;
using Modules.Contracts.DTOs.Storage;

namespace Modules.Application.Queries.Storage;

public sealed record GetStorageConnectionsQuery : IRequest<List<StorageConnectionDto>>;
