using Domeo.Shared.Exceptions;
using MediatR;
using Projects.Abstractions.Commands.Projects;
using Projects.Abstractions.DTOs;
using Projects.Abstractions.Repositories;

namespace Projects.API.Application.Projects.Queries;

public sealed class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            throw new NotFoundException("Project", request.Id);

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Type,
            project.Status.ToString(),
            project.ClientId,
            project.UserId,
            project.Notes,
            project.QuestionnaireData,
            project.CreatedAt,
            project.UpdatedAt,
            project.DeletedAt);
    }
}
