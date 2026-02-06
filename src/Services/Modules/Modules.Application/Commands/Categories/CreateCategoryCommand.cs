using MediatR;
using Modules.Contracts.DTOs.Categories;

namespace Modules.Application.Commands.Categories;

public sealed record CreateCategoryCommand(
    string Id,
    string Name,
    string? ParentId,
    string? Description,
    int OrderIndex) : IRequest<ModuleCategoryDto>;
