using MediatR;

namespace Projects.Application.Commands.Projects;

public sealed record UpdateProjectStatusCommand(
    Guid Id,
    string Status) : IRequest;
