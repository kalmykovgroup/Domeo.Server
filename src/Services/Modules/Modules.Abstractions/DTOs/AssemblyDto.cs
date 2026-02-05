using Modules.Abstractions.Entities.Shared;

namespace Modules.Abstractions.DTOs;

public sealed record AssemblyDto(
    Guid Id,
    string CategoryId,
    string Type,
    string Name,
    Dimensions Dimensions,
    Constraints? Constraints,
    Construction? Construction,
    bool IsActive,
    DateTime CreatedAt);
