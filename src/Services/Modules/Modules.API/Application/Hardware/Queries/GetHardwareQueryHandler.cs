using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Modules.Abstractions.Queries.Hardware;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.Hardware.Queries;

public sealed class GetHardwareQueryHandler : IQueryHandler<GetHardwareQuery, List<HardwareDto>>
{
    private readonly IHardwareRepository _repository;

    public GetHardwareQueryHandler(IHardwareRepository repository)
        => _repository = repository;

    public async Task<Result<List<HardwareDto>>> Handle(
        GetHardwareQuery request, CancellationToken cancellationToken)
    {
        var hardware = await _repository.GetHardwareAsync(request.Type, request.ActiveOnly, cancellationToken);

        var dtos = hardware.Select(h => new HardwareDto(
            h.Id,
            h.Type,
            h.Name,
            h.Brand,
            h.Model,
            h.ModelUrl,
            h.Params,
            h.IsActive)).ToList();

        return Result.Success(dtos);
    }
}
