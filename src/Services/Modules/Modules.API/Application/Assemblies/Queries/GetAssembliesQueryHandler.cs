using MediatR;
using Modules.Abstractions.DTOs;
using Modules.Abstractions.Entities;
using Modules.Abstractions.Queries.Assemblies;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.Assemblies.Queries;

public sealed class GetAssembliesQueryHandler : IRequestHandler<GetAssembliesQuery, AssembliesResponse>
{
    private readonly IAssemblyRepository _repository;

    public GetAssembliesQueryHandler(IAssemblyRepository repository)
        => _repository = repository;

    public async Task<AssembliesResponse> Handle(
        GetAssembliesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _repository.GetAssembliesAsync(
            request.CategoryId, request.ActiveOnly, request.Search,
            request.Page, request.Limit, cancellationToken);

        var dtos = items.Select(ToDto).ToList();

        return request.Page.HasValue && request.Limit.HasValue
            ? new AssembliesResponse(dtos, total, request.Page, request.Limit)
            : new AssembliesResponse(dtos);
    }

    private static AssemblyDto ToDto(Assembly a) => new(
        a.Id,
        a.CategoryId,
        a.Type,
        a.Name,
        a.Dimensions,
        a.Constraints,
        a.Construction,
        a.IsActive,
        a.CreatedAt);
}
