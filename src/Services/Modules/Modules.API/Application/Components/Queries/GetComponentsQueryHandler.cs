using MediatR;
using Modules.Application.Queries.Components;
using Modules.Contracts.DTOs.Components;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Components.Queries;

public sealed class GetComponentsQueryHandler : IRequestHandler<GetComponentsQuery, List<ComponentDto>>
{
    private readonly IComponentRepository _repository;

    public GetComponentsQueryHandler(IComponentRepository repository)
        => _repository = repository;

    public async Task<List<ComponentDto>> Handle(
        GetComponentsQuery request, CancellationToken cancellationToken)
    {
        var components = await _repository.GetComponentsAsync(request.Tag, request.ActiveOnly, cancellationToken);

        return components.Select(c => new ComponentDto(
            c.Id,
            c.Name,
            c.Tags,
            c.Params,
            c.IsActive,
            c.CreatedAt)).ToList();
    }
}
