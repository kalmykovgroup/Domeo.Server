using MediatR;
using Modules.Abstractions.DTOs;
using Modules.Abstractions.Queries.Hardware;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.Hardware.Queries;

public sealed class GetHardwareQueryHandler : IRequestHandler<GetHardwareQuery, List<HardwareDto>>
{
    private readonly IHardwareRepository _repository;

    public GetHardwareQueryHandler(IHardwareRepository repository)
        => _repository = repository;

    public async Task<List<HardwareDto>> Handle(
        GetHardwareQuery request, CancellationToken cancellationToken)
    {
        var hardware = await _repository.GetHardwareAsync(request.Type, request.ActiveOnly, cancellationToken);

        return hardware.Select(h => new HardwareDto(
            h.Id,
            h.Type,
            h.Name,
            h.Brand,
            h.Model,
            h.ModelUrl,
            h.Params,
            h.IsActive)).ToList();
    }
}
