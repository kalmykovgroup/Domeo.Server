using MediatR;
using Modules.Abstractions.DTOs;

namespace Modules.Abstractions.Queries.ModuleTypes;

public sealed record GetModuleTypeByIdQuery(int Id) : IRequest<ModuleTypeDto>;
