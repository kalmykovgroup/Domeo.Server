using Modules.Contracts.DTOs.AssemblyParts;
using Modules.Domain.Entities.Shared;

namespace Modules.Contracts.DTOs.Assemblies;

public sealed record AssemblyDto(
    Guid Id,
    string CategoryId,
    string Type,
    string Name,
    Dimensions Dimensions,
    Constraints? Constraints,
    Construction? Construction,
    bool IsActive,
    DateTime CreatedAt,
    List<AssemblyPartDto> Parts);
