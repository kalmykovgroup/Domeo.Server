using Materials.Abstractions.DTOs;
using Materials.Abstractions.ExternalServices;
using Materials.Abstractions.Queries.Suppliers;
using MediatR;

namespace Materials.API.Application.Suppliers.Queries;

public sealed class GetSuppliersQueryHandler : IRequestHandler<GetSuppliersQuery, List<SupplierDto>>
{
    private readonly ISupplierApiClient _supplierApiClient;
    private readonly ILogger<GetSuppliersQueryHandler> _logger;

    public GetSuppliersQueryHandler(
        ISupplierApiClient supplierApiClient,
        ILogger<GetSuppliersQueryHandler> logger)
    {
        _supplierApiClient = supplierApiClient;
        _logger = logger;
    }

    public async Task<List<SupplierDto>> Handle(
        GetSuppliersQuery request,
        CancellationToken cancellationToken)
    {
        var externalSuppliers = await _supplierApiClient.GetSuppliersAsync(
            request.ActiveOnly ?? true, cancellationToken);

        var suppliers = externalSuppliers.Select(s => new SupplierDto(
            s.Id,
            s.Company,
            s.ContactName,
            s.Email,
            s.Phone,
            s.Address,
            s.Website,
            s.Rating,
            s.IsActive)).ToList();

        return suppliers;
    }
}
