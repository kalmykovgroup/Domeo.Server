using MediatR;

namespace Projects.Abstractions.Commands.Projects;

public sealed record UpdateProjectCommand(
    Guid Id,
    string Name,
    string? Notes) : IRequest;
