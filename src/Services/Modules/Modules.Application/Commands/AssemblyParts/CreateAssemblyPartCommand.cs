using MediatR;
using Modules.Contracts.DTOs.AssemblyParts;
using Modules.Domain.Entities.Shared;

namespace Modules.Application.Commands.AssemblyParts;

public sealed record CreateAssemblyPartCommand(
    Guid AssemblyId,
    Guid ComponentId,
    PartRole Role,
    Placement Placement,
    DynamicSize? Length,
    DynamicSize? Width,
    List<ShapeSegment>? Shape,
    int Quantity,
    string? QuantityFormula,
    int SortOrder) : IRequest<AssemblyPartDto>;
