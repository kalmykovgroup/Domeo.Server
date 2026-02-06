using Modules.Domain.Entities.Shared;

namespace Modules.Contracts.DTOs.Components;

public sealed record CreateComponentRequest(
    string Name,
    ComponentParams? Params,
    List<string>? Tags);
