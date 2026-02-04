using Auth.Contracts;
using Domeo.Shared.Contracts;
using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Abstractions.Commands.Projects;
using Projects.Abstractions.Entities;
using Projects.Abstractions.Repositories;
using ProjectDto = Projects.Abstractions.DTOs.ProjectDto;

namespace Projects.API.Application.Projects.Queries;

public sealed class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PaginatedResponse<ProjectDto>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public GetProjectsQueryHandler(
        IProjectRepository projectRepository,
        ICurrentUserAccessor currentUserAccessor)
    {
        _projectRepository = projectRepository;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<PaginatedResponse<ProjectDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id;
        if (userId is null)
            throw new UnauthorizedException();

        ProjectStatus? status = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<ProjectStatus>(request.Status, true, out var statusEnum))
            status = statusEnum;

        var sortDesc = string.Equals(request.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);

        var (items, total) = await _projectRepository.GetProjectsAsync(
            userId.Value,
            request.ClientId,
            request.Search,
            status,
            request.Type,
            request.Page,
            request.PageSize,
            request.SortBy,
            sortDesc,
            cancellationToken);

        var dtos = items.Select(p => new ProjectDto(
            p.Id,
            p.Name,
            p.Type,
            p.Status.ToString(),
            p.ClientId,
            p.UserId,
            p.Notes,
            p.QuestionnaireData,
            p.CreatedAt,
            p.UpdatedAt,
            p.DeletedAt)).ToList();

        return new PaginatedResponse<ProjectDto>(total, request.Page, request.PageSize, dtos);
    }
}
