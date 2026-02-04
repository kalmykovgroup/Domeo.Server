using MediatR;

namespace Projects.Abstractions.Commands.Projects;

public sealed record UpdateProjectStatusCommand(
    Guid Id,
    string Status) : IRequest;
