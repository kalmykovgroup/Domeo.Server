using Modules.Abstractions.Entities.Shared;

namespace Modules.Abstractions.DTOs;

public sealed record AssemblyDetailDto(
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
