using MediatR;
using Modules.Abstractions.DTOs;

namespace Modules.Abstractions.Queries.Hardware;

public sealed record GetHardwareQuery(string? Type, bool? ActiveOnly) : IRequest<List<HardwareDto>>;
