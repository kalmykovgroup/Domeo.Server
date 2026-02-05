using System.Security.Claims;
using Domeo.Shared.Application;
using Domeo.Shared.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Projects.Application.Commands.Projects;
using Projects.Domain.Entities;
using Projects.Domain.Repositories;

namespace Projects.API.Application.Projects.Commands;

public sealed class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedException();

        var project = Project.Create(
            request.Name,
            request.Type,
            request.ClientId,
            userId,
            request.Notes);

        _projectRepository.Add(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
