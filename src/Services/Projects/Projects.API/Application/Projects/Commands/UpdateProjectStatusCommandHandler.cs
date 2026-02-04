using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Projects.Abstractions.Commands.Projects;
using Projects.Abstractions.Entities;
using Projects.Abstractions.Repositories;

namespace Projects.API.Application.Projects.Commands;

public sealed class UpdateProjectStatusCommandHandler : ICommandHandler<UpdateProjectStatusCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectStatusCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProjectStatusCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            return Result.Failure(Error.NotFound("Project", request.Id));

        if (!Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
            return Result.Failure(Error.Validation("Project.InvalidStatus", "Invalid status"));

        project.UpdateStatus(status);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
