using MediatR;

namespace Modules.Application.Commands.Assemblies;

public sealed record DeleteAssemblyCommand(Guid Id) : IRequest;
