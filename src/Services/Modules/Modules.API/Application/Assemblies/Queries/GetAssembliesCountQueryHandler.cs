using MediatR;
using Modules.Abstractions.Queries.Assemblies;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.Assemblies.Queries;

public sealed class GetAssembliesCountQueryHandler : IRequestHandler<GetAssembliesCountQuery, int>
{
    private readonly IAssemblyRepository _repository;

    public GetAssembliesCountQueryHandler(IAssemblyRepository repository)
        => _repository = repository;

    public async Task<int> Handle(
        GetAssembliesCountQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetAssembliesCountAsync(
            request.CategoryId, request.ActiveOnly, request.Search, cancellationToken);
    }
}
