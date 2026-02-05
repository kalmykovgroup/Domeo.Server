using MediatR;
using Modules.Abstractions.DTOs;
using Modules.Abstractions.Queries.AssemblyParts;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.AssemblyParts.Queries;

public sealed class GetAssemblyPartsQueryHandler : IRequestHandler<GetAssemblyPartsQuery, List<AssemblyPartDto>>
{
    private readonly IAssemblyPartRepository _repository;

    public GetAssemblyPartsQueryHandler(IAssemblyPartRepository repository)
        => _repository = repository;

    public async Task<List<AssemblyPartDto>> Handle(
        GetAssemblyPartsQuery request, CancellationToken cancellationToken)
    {
        var parts = await _repository.GetByAssemblyIdAsync(request.AssemblyId, cancellationToken);

        return parts.Select(p => new AssemblyPartDto(
            p.Id,
            p.AssemblyId,
            p.ComponentId,
            p.Role.ToString(),
            p.Length,
            p.Width,
            p.Placement,
            p.Quantity,
            p.QuantityFormula,
            p.SortOrder)).ToList();
    }
}
