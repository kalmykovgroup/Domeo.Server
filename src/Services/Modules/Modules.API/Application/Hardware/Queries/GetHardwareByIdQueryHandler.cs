using Domeo.Shared.Contracts.DTOs;
using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Modules.Abstractions.Queries.Hardware;
using Modules.Abstractions.Repositories;

namespace Modules.API.Application.Hardware.Queries;

public sealed class GetHardwareByIdQueryHandler : IQueryHandler<GetHardwareByIdQuery, HardwareDto>
{
    private readonly IHardwareRepository _repository;

    public GetHardwareByIdQueryHandler(IHardwareRepository repository)
        => _repository = repository;

    public async Task<Result<HardwareDto>> Handle(
        GetHardwareByIdQuery request, CancellationToken cancellationToken)
    {
        var hw = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (hw is null)
            return Result.Failure<HardwareDto>(Error.NotFound("Hardware.NotFound", "Hardware not found"));

        return Result.Success(new HardwareDto(
            hw.Id,
            hw.Type,
            hw.Name,
            hw.Brand,
            hw.Model,
            hw.ModelUrl,
            hw.Params,
            hw.IsActive));
    }
}
