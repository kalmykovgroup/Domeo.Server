using MediatR;
using Modules.Abstractions.Queries.ModuleTypes;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.ModuleTypes.Queries;

public sealed class GetModuleTypesCountQueryHandler : IRequestHandler<GetModuleTypesCountQuery, int>
{
    private readonly IModuleTypeRepository _repository;

    public GetModuleTypesCountQueryHandler(IModuleTypeRepository repository)
        => _repository = repository;

    public async Task<int> Handle(
        GetModuleTypesCountQuery request, CancellationToken cancellationToken)
    {
        return await _repository.GetModuleTypesCountAsync(
            request.CategoryId, request.ActiveOnly, request.Search, cancellationToken);
    }
}
