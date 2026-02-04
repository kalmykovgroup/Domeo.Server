using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Clients.Abstractions.Queries;

public sealed record GetClientsQuery(
    string? Search,
    int? Page,
    int? PageSize,
    string? SortBy,
    string? SortOrder) : IQuery<PaginatedResponse<ClientDto>>;
