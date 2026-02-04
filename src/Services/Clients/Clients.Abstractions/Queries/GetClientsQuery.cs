using Clients.Abstractions.DTOs;
using Domeo.Shared.Contracts;
using MediatR;

namespace Clients.Abstractions.Queries;

public sealed record GetClientsQuery(
    string? Search,
    int? Page,
    int? PageSize,
    string? SortBy,
    string? SortOrder) : IRequest<PaginatedResponse<ClientDto>>;
