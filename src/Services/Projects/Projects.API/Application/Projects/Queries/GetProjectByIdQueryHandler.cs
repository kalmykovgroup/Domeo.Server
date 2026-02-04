using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Projects.Abstractions.Commands.Projects;
using Projects.Abstractions.Repositories;

namespace Projects.API.Application.Projects.Queries;

public sealed class GetProjectByIdQueryHandler : IQueryHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            return Result.Failure<ProjectDto>(Error.NotFound("Project", request.Id));

        var dto = new ProjectDto(
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

        return Result.Success(dto);
    }
}
