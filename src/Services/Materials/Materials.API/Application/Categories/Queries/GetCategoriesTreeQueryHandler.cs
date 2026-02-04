using Domeo.Shared.Kernel.Application.Abstractions;
using Domeo.Shared.Kernel.Domain.Results;
using Materials.Abstractions.DTOs;
using Materials.Abstractions.ExternalServices;
using Materials.Abstractions.Queries.Categories;

namespace Materials.API.Application.Categories.Queries;

public sealed class GetCategoriesTreeQueryHandler : IQueryHandler<GetCategoriesTreeQuery, List<CategoryTreeNodeDto>>
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

    public async Task<Result<List<CategoryTreeNodeDto>>> Handle(
        GetCategoriesTreeQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var externalTree = await _supplierApiClient.GetCategoriesTreeAsync(
                request.ActiveOnly ?? true, cancellationToken);

            var tree = externalTree.Select(MapCategoryTreeNode).ToList();

            return Result.Success(tree);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Supplier service unavailable");
            return Result.Failure<List<CategoryTreeNodeDto>>(
                Error.ServiceUnavailable($"Supplier service unavailable: {ex.Message}"));
        }
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
