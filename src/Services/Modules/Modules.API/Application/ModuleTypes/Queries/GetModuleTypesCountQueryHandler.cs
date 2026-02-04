using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Modules.Abstractions.Queries.ModuleTypes;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.ModuleTypes.Queries;

public sealed class GetModuleTypesCountQueryHandler : IQueryHandler<GetModuleTypesCountQuery, int>
{
    private readonly IModuleTypeRepository _repository;

    public GetModuleTypesCountQueryHandler(IModuleTypeRepository repository)
        => _repository = repository;

    public async Task<Result<int>> Handle(
        GetModuleTypesCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _repository.GetModuleTypesCountAsync(
            request.CategoryId, request.ActiveOnly, request.Search, cancellationToken);

        return Result.Success(count);
    }
}
