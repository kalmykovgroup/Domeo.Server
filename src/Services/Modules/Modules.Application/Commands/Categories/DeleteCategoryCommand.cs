using MediatR;

namespace Modules.Application.Commands.Categories;

public sealed record DeleteCategoryCommand(string Id) : IRequest;
