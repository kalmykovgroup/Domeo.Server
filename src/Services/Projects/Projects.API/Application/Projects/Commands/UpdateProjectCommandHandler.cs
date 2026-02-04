using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Projects.Abstractions.Commands.Projects;
using Projects.Abstractions.Repositories;

namespace Projects.API.Application.Projects.Commands;

public sealed class UpdateProjectCommandHandler : ICommandHandler<UpdateProjectCommand>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _projectRepository.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
            return Result.Failure(Error.NotFound("Project", request.Id));

        project.Update(request.Name, request.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
