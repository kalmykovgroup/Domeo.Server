using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Projects.Abstractions.Commands.Projects;

public sealed record GetProjectByIdQuery(Guid Id) : IQuery<ProjectDto>;
