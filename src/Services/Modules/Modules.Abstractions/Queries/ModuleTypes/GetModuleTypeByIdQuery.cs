using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;

namespace Modules.Abstractions.Queries.ModuleTypes;

public sealed record GetModuleTypeByIdQuery(int Id) : IQuery<ModuleTypeDto>;
