using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Projects.Abstractions.Commands.Projects;

public sealed record GetProjectsQuery(
    Guid? ClientId,
    string? Search,
    string? Status,
    string? Type,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortOrder = null) : IQuery<PaginatedResponse<ProjectDto>>;
