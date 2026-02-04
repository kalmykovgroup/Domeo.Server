using Domeo.Shared.Exceptions;
using MediatR;
using Modules.Abstractions.DTOs;
using Modules.Abstractions.Queries.Hardware;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.Hardware.Queries;

public sealed class GetHardwareByIdQueryHandler : IRequestHandler<GetHardwareByIdQuery, HardwareDto>
{
    private readonly IHardwareRepository _repository;

    public GetHardwareByIdQueryHandler(IHardwareRepository repository)
        => _repository = repository;

    public async Task<HardwareDto> Handle(
        GetHardwareByIdQuery request, CancellationToken cancellationToken)
    {
        var hw = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (hw is null)
            throw new NotFoundException("Hardware", request.Id);

        return new HardwareDto(
            hw.Id,
            hw.Type,
            hw.Name,
            hw.Brand,
            hw.Model,
            hw.ModelUrl,
            hw.Params,
            hw.IsActive);
    }
}
