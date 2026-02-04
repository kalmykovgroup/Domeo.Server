using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Modules.Abstractions.Queries.Hardware;

public sealed record GetHardwareQuery(string? Type, bool? ActiveOnly) : IQuery<List<HardwareDto>>;
