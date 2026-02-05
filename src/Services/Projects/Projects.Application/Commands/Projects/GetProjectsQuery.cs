using Domeo.Shared.Contracts;
using MediatR;
using Projects.Contracts.DTOs.Projects;

namespace Projects.Application.Commands.Projects;

public sealed record GetProjectsQuery(
    Guid? ClientId,
    string? Search,
    string? Status,
    string? Type,
    int Page = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortOrder = null) : IRequest<PaginatedResponse<ProjectDto>>;
