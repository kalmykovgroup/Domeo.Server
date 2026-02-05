using Domeo.Shared.Exceptions;
using Materials.Contracts.DTOs;
using Materials.Application.ExternalServices;
using Materials.Application.Queries.Suppliers;
using MediatR;

namespace Materials.API.Application.Suppliers.Queries;

public sealed class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, SupplierDto>
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

    public async Task<SupplierDto> Handle(
        GetSupplierByIdQuery request,
        CancellationToken cancellationToken)
    {
        var externalSupplier = await _supplierApiClient.GetSupplierAsync(request.Id, cancellationToken);

        if (externalSupplier == null)
            throw new NotFoundException("Supplier", request.Id);

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

        return supplier;
    }
}
