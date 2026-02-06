using MediatR;
using Modules.Application.Queries.Components;
using Modules.Contracts.DTOs.Components;
using Modules.Domain.Repositories;

namespace Modules.API.Application.Components.Queries;

public sealed class GetComponentByIdQueryHandler : IRequestHandler<GetComponentByIdQuery, ComponentDto?>
{
    private readonly IComponentRepository _repository;

    public GetComponentByIdQueryHandler(IComponentRepository repository)
        => _repository = repository;

    public async Task<ComponentDto?> Handle(
        GetComponentByIdQuery request, CancellationToken cancellationToken)
    {
        var component = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (component is null) return null;

        return new ComponentDto(
            component.Id,
            component.Name,
            component.Tags,
            component.Params,
            component.IsActive,
            component.CreatedAt);
    }
}
