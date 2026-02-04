using MediatR;
using Projects.Abstractions.DTOs;

namespace Projects.Abstractions.Commands.Projects;

public sealed record GetProjectByIdQuery(Guid Id) : IRequest<ProjectDto>;
