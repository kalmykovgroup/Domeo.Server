using MediatR;
using Modules.Contracts.DTOs.Components;
using Modules.Domain.Entities.Shared;

namespace Modules.Application.Commands.Components;

public sealed record UpdateComponentCommand(
    Guid Id,
    string Name,
    ComponentParams? Params,
    List<string>? Tags,
    string? Color) : IRequest<ComponentDto>;
