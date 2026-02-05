using Materials.Contracts.DTOs;
using Materials.Application.ExternalServices;
using Materials.Application.Queries.Categories;
using MediatR;

namespace Materials.API.Application.Categories.Queries;

public sealed class GetCategoriesTreeQueryHandler : IRequestHandler<GetCategoriesTreeQuery, List<CategoryTreeNodeDto>>
{
    private readonly ISupplierApiClient _supplierApiClient;
    private readonly ILogger<GetCategoriesTreeQueryHandler> _logger;

    public GetCategoriesTreeQueryHandler(
        ISupplierApiClient supplierApiClient,
        ILogger<GetCategoriesTreeQueryHandler> logger)
    {
        _supplierApiClient = supplierApiClient;
        _logger = logger;
    }

    public async Task<List<CategoryTreeNodeDto>> Handle(
        GetCategoriesTreeQuery request,
        CancellationToken cancellationToken)
    {
        var externalTree = await _supplierApiClient.GetCategoriesTreeAsync(
            request.ActiveOnly ?? true, cancellationToken);

        var tree = externalTree.Select(MapCategoryTreeNode).ToList();

        return tree;
    }

    private static CategoryTreeNodeDto MapCategoryTreeNode(ExternalCategoryTreeNode node)
    {
        return new CategoryTreeNodeDto
        {
            Id = node.Id,
            ParentId = node.ParentId,
            Name = node.Name,
            Level = node.Level,
            OrderIndex = node.OrderIndex,
            IsActive = node.IsActive,
            SupplierIds = node.SupplierIds,
            Children = node.Children.Select(MapCategoryTreeNode).ToList()
        };
    }
}
