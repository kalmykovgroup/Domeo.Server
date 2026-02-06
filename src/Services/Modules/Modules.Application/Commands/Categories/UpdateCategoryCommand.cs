using MediatR;
using Modules.Contracts.DTOs.Categories;

namespace Modules.Application.Commands.Categories;

public sealed record UpdateCategoryCommand(
    string Id,
    string Name,
    string? Description,
    int OrderIndex) : IRequest<ModuleCategoryDto>;
