using MediatR;
using Projects.Contracts.DTOs.Projects;

namespace Projects.Application.Commands.Projects;

public sealed record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto>;
