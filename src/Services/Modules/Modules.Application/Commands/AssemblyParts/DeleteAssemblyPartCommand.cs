using MediatR;

namespace Modules.Application.Commands.AssemblyParts;

public sealed record DeleteAssemblyPartCommand(Guid Id) : IRequest;
