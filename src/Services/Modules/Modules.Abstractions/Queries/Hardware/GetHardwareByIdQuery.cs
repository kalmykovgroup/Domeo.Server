using MediatR;
using Modules.Abstractions.DTOs;

namespace Modules.Abstractions.Queries.Hardware;

public sealed record GetHardwareByIdQuery(int Id) : IRequest<HardwareDto>;
