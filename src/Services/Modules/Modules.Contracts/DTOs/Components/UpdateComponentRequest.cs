using Modules.Domain.Entities.Shared;

namespace Modules.Contracts.DTOs.Components;

public sealed record UpdateComponentRequest(
    string Name,
    ComponentParams? Params,
    List<string>? Tags);
