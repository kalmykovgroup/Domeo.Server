using Materials.Contracts.DTOs;
using Materials.Application.ExternalServices;
using Materials.Application.Queries.Categories;
using MediatR;

namespace Materials.API.Application.Categories.Queries;

public sealed class GetCategoryAttributesQueryHandler : IRequestHandler<GetCategoryAttributesQuery, List<CategoryAttributeDto>>
{
    private readonly ISupplierApiClient _supplierApiClient;
    private readonly ILogger<GetCategoryAttributesQueryHandler> _logger;

    public GetCategoryAttributesQueryHandler(
        ISupplierApiClient supplierApiClient,
        ILogger<GetCategoryAttributesQueryHandler> logger)
    {
        _supplierApiClient = supplierApiClient;
        _logger = logger;
    }

    public async Task<List<CategoryAttributeDto>> Handle(
        GetCategoryAttributesQuery request,
        CancellationToken cancellationToken)
    {
        var externalAttributes = await _supplierApiClient.GetCategoryAttributesAsync(
            request.CategoryId, cancellationToken);

        return externalAttributes.Select(a => new CategoryAttributeDto(
            a.Id,
            a.CategoryId,
            a.Name,
            a.Type,
            a.Unit,
            a.EnumValues)).ToList();
    }
}
