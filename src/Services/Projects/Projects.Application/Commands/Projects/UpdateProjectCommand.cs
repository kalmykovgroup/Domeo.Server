using MediatR;

namespace Projects.Application.Commands.Projects;

public sealed record UpdateProjectCommand(
    Guid Id,
    string Name,
    string? Notes) : IRequest;
