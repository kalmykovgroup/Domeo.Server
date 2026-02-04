using Domeo.Shared.Auth;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Projects.Abstractions.Commands.Projects;
using Projects.Abstractions.Entities;
using Projects.Abstractions.Repositories;

namespace Projects.API.Application.Projects.Commands;

public sealed class CreateProjectCommandHandler : ICommandHandler<CreateProjectCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserAccessor currentUserAccessor)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<Result<Guid>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserAccessor.User?.Id;
        if (userId is null)
            return Result.Failure<Guid>(Error.Failure("User.Unauthorized", "User not authenticated"));

        var project = Project.Create(
            request.Name,
            request.Type,
            request.ClientId,
            userId.Value,
            request.Notes);

        _projectRepository.Add(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(project.Id);
    }
}
