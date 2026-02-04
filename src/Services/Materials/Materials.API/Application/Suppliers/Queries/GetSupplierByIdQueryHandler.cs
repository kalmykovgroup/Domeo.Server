using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.ExternalServices;
using Materials.Abstractions.Queries.Suppliers;

namespace Materials.API.Application.Suppliers.Queries;

public sealed class GetSupplierByIdQueryHandler : IQueryHandler<GetSupplierByIdQuery, SupplierDto>
{
    private readonly ISupplierApiClient _supplierApiClient;
    private readonly ILogger<GetSupplierByIdQueryHandler> _logger;

    public GetSupplierByIdQueryHandler(
        ISupplierApiClient supplierApiClient,
        ILogger<GetSupplierByIdQueryHandler> logger)
    {
        _supplierApiClient = supplierApiClient;
        _logger = logger;
    }

    public async Task<Result<SupplierDto>> Handle(
        GetSupplierByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var externalSupplier = await _supplierApiClient.GetSupplierAsync(request.Id, cancellationToken);

            if (externalSupplier == null)
                return Result.Failure<SupplierDto>(Error.NotFound("Supplier not found"));

            var supplier = new SupplierDto(
                externalSupplier.Id,
                externalSupplier.Company,
                externalSupplier.ContactName,
                externalSupplier.Email,
                externalSupplier.Phone,
                externalSupplier.Address,
                externalSupplier.Website,
                externalSupplier.Rating,
                externalSupplier.IsActive);

            return Result.Success(supplier);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Supplier service unavailable");
            return Result.Failure<SupplierDto>(
                Error.ServiceUnavailable($"Supplier service unavailable: {ex.Message}"));
        }
    }
}
