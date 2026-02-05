using MediatR;

namespace Projects.Application.Commands.Projects;

public sealed record DeleteProjectCommand(Guid Id) : IRequest;
